using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using Emotiv;
using UnityEngine;
//using computeAttention;


public class EEGLogger {

   //[DllImport("edk")]
    //public static extern void EDK_cleanup();

    public int readCount;

    EmoEngine engine;
    
        //(assemblies);
    private static int userID = -1;    
    private string outputDataFile =  "E:\\luyuhao\\testdata\\RawSignals\\" + DateTime.Now.ToString("ddMMyyyy") + ".csv";
    private static int bufferSizeLimit = 64;
    private double[][] rawSignals = new double[4][];

    public static bool SetThresholds;
    private double thresholdR;
    private double threshold;
    private double BTconcentrationLevel;
    private static float startTime;
    private static bool testMode = true;


    // Use this for initialization
    public EEGLogger() {

        engine = EmoEngine.Instance;

        //Debug.Log(engine);
        threshold = 0;
        thresholdR = 0;
        readCount = 0;
        SetThresholds = false;

        for (int i = 0; i < 4; i++) {
            rawSignals[i] = new double[bufferSizeLimit];
        }

        engine.UserAdded += new EmoEngine.UserAddedEventHandler(engine_UserAdded);
        engine.UserRemoved += new EmoEngine.UserRemovedEventHandler(engine_UserRemoved);
        engine.Connect();

	}

    void engine_UserAdded(object sender, EmoEngineEventArgs e) {
        Console.WriteLine("Dongle Plugged!");
        userID = (int)e.userId;

        //enable the data aquisition for this user
        engine.DataAcquisitionEnable((uint)userID, true);
        //ask for up to 1s of buffered data
        engine.EE_DataSetBufferSizeInSec(1);
    }

    void engine_UserRemoved(object sender, EmoEngineEventArgs e)
    {
        Console.WriteLine("Dongle Removed!");
        userID = -1;
    }

    public static void OnRetrieveData() {

        Debug.Log("OnRetrieveData Thread Created!");
        
        EEGLogger logger = new EEGLogger();


        TextWriter file;
        float attentionSc = -1;
        int attentionLv = -1;

        string root_path= Environment.CurrentDirectory;
        string path = Directory.GetParent(root_path).ToString();//Directory.GetCurrentDirectory()//.Parent
        string dataLocation = path+ "\\testdata\\" + DateTime.Now.ToString(@"yyyyddMM_HHmm") + ".txt";

        //set up threshold for attention
        if (testMode)
        {
            logger.setThresholds();
        }
        else {


        }
        
        
        try
        {
            file = new StreamWriter(dataLocation, true);
            file.WriteLine("AttentionScore , AttentionLevel ");
            file.Close();
        }
        catch (Exception e) {
            Debug.Log("problem writing attention data!");
            Debug.Log(e);
        }
        

        while (true)
        {
            Thread.Sleep(250);

            if (testMode) {
                file = new StreamWriter(dataLocation, true);
                logger.readCount++;
                attentionSc = (float)logger.generateFakeAttention(); //0-100
                attentionLv = logger.CalAttensionLevel(attentionSc); ;
                Player.attentionScore = attentionSc;
                Player.attentionLvl = attentionLv;
                Player.UpdateHorizontalSpeed();
                file.WriteLine(attentionSc + ",  " + attentionLv);
                file.Close();

            } else {
                if (userID > 0)
                {

                    if (logger.Run())
                    {
                        file = new StreamWriter(dataLocation, true);
                        logger.readCount++;
                        logger.TimeToFrq32();
                        attentionSc = (float)logger.CalculateAttention();
                        attentionLv = logger.CalAttensionLevel(attentionSc); ;
                        Player.attentionScore = attentionSc;
                        Player.attentionLvl = attentionLv;

                        file.WriteLine(attentionSc + ",  " + attentionLv);
                        file.Close();
                        //TODO: Update UI display
                    }
                }
            }
            
        }
        
    }


    /*1)retrieve data signals
     * 2)record signal into files
     3)Compute attentionScore*/
    private bool Run() {

        // Handle any waiting events
        engine.ProcessEvents();

        // If the user has not yet connected, do not proceed
        if ((int)userID == -1)
        {
            return false;
        }
        Dictionary<EdkDll.EE_DataChannel_t, double[]> data = engine.GetData((uint)userID);

        if (data == null)
        {
            return false;
        }

        int _bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;

        // Write the data to a file
        TextWriter file = new StreamWriter(outputDataFile, true);

        //WriteHeader();
        file.WriteLine("COUNTER, INTERPOLATED,RAW_CQ,AF3,F7,F3,FC5,T7,P7,O1,O2,P8,T8,FC6,F4,F8,AF4,GYROX,GYROY,TIMESTAMP,ES_TIMESTAMP,FUNC_ID,FUNC_VALUE,MARKER,SYNC_SIGNAL");

        string line;
        //convert data into matrix for computation
        
        int count = 0;

        for (int i = 0; i < _bufferSize; i++)
        {
            if (i > 31) { break; }
            
            //  write the data into files
            line = "";
            foreach (EdkDll.EE_DataChannel_t channel in data.Keys)
            {

                line += data[channel][i] + ",";
                count++;
            }

            rawSignals[0][i] = data[EdkDll.EE_DataChannel_t.AF3][i];
            rawSignals[1][i] = data[EdkDll.EE_DataChannel_t.O1][i];
            rawSignals[2][i] = data[EdkDll.EE_DataChannel_t.O2][i];
            rawSignals[3][i] = data[EdkDll.EE_DataChannel_t.AF4][i];

            file.WriteLine(line);
        }

        file.Close();
        return true;
    }

    private int generateFakeAttention() {
        System.Random rnd = new System.Random();
        int result = rnd.Next(0, 100);
        return result;
    }
    private void setThresholds() {

        /*int counter;

        for (counter = 0; counter < 127; counter++) {
            if (Run())
            {
                TimeToFrq32();
                thresholdR += CalculateAttention();
            }
            counter++;
        }

         thresholdR = thresholdR / 128;
         Console.WriteLine("thresholdR = " + thresholdR);
        

        if (Run())
        {
            TimeToFrq32();
            threshold += CalculateAttention();

        }

            threshold = threshold / 128;
            Console.WriteLine("threshold for focus = " + threshold);
        
        threshold = threshold + (thresholdR - threshold) * (1 / 3);*/

        //create fake threshold 
        threshold = 80;
        thresholdR = 50;
    }

    private float CalculateAttention()
    {
        // int Fs = 128;
        // int N = 64;
        // theta : 4 ~ 8 Hz                    [2]~[4]                                 
        // alpha : 8 ~ 13Hz = num * Fs/N       [4]~[6]     
        // beta  : 13 ~ 20Hz                   [7]~[10]     
        // gama  : above 30 Hz                   [15+]
        double alpha, beta, theta;
        theta = rawSignals[0][2] + rawSignals[0][3] + rawSignals[0][4] +
                rawSignals[1][2] + rawSignals[1][3] + rawSignals[1][4] +
                rawSignals[2][2] + rawSignals[2][3] + rawSignals[2][4] +
                rawSignals[3][2] + rawSignals[3][3] + rawSignals[3][4] ;

        theta = theta / 4;

        alpha = rawSignals[0][4] + rawSignals[0][5] + rawSignals[0][6] +
                rawSignals[1][4] + rawSignals[1][5] + rawSignals[1][6] +
                rawSignals[2][4] + rawSignals[2][5] + rawSignals[2][6] +
                rawSignals[3][4] + rawSignals[3][5] + rawSignals[3][6];

        alpha = alpha / 4;

        beta = rawSignals[0][7] + rawSignals[0][8] + rawSignals[0][9] + rawSignals[0][10] +
                rawSignals[1][7] + rawSignals[1][8] + rawSignals[1][9] + rawSignals[0][10] +
                rawSignals[2][7] + rawSignals[2][8] + rawSignals[2][9] + rawSignals[0][10] +
                rawSignals[3][7] + rawSignals[3][8] + rawSignals[3][9];

        beta = beta / 4;
        // concentrationLevel in dB
        float attentionScore = (float)(20 * Math.Log(theta / beta));
        return attentionScore;
    }

    private double[] ComputeAttentionScore(int bufferSize, double[,] data) {
        //TODO: use matlab code to compute attention scores

        /*//pass the data to matlab
        MLApp.MLApp matlab = new MLApp.MLApp();

        // Change to the directory where the function is located 
        matlab.Execute(@"cd E:\luyuhao\matlab");

        // Define the output 
        object result = null;

        // Call the MATLAB function myfunc
        matlab.Feval("computeAttention", 1, out result, data);

        // Display result 
        object[] res = result as object[];

        Console.WriteLine("Attention Score = " + result);
        //Console.WriteLine(res[1]);
        //Console.ReadLine(); 
        return Convert.ToInt32(result);
        */

        /*
         Console.WriteLine("Use .m");

            // Instantiate our .NET class from the MATLAB created component
            dotnetclass AClass = new dotnetclass();

            // explicity convert our input arguments into MWArrays
            // this can be done with implicit conversion
            MWNumericArray a = new MWNumericArray(1, 2, (double[])ar, (double[])ai);
            MWNumericArray b = new MWNumericArray(1, 2, (double[])br, (double[])bi);

            // call math_on_method from Assembly specifying the number
            // of return arguments expected and passing in a and b
            MWArray[] RetVal = AClass.math_on_numbers(2, a, b);

            // Unpack return values seperating the real and imaginary parts
            // using the ToArray method of MWNummericArray.  Since RetVal was
            // declared as a MWArray above, it must be explicity typecast here.  
            cr = ((MWNumericArray) RetVal[0]).ToVector(MWArrayComponent.Real);
            ci = ((MWNumericArray) RetVal[0]).ToVector(MWArrayComponent.Imaginary);
        
            dr = ((MWNumericArray) RetVal[1]).ToVector(MWArrayComponent.Real);
            di = ((MWNumericArray) RetVal[1]).ToVector(MWArrayComponent.Imaginary);*/

        double[] results = new double[2]; //[attentionScore, strength]
        TimeToFrq32();
        results[0] = CalculateAttention();
        //file.WriteLine("A: " + x + ",");
        //results[1] = CalAttensionLevel();
        //file.WriteLine("S: " + strength + ",");
        //Console.WriteLine("compute Attention SCORES!");
        return results;
    }

    private void TimeToFrq32()
    {
        double[] tempData;
        tempData = new double[64];

        //AF3
        for (int ii = 0; ii < 32; ii++)
        {
            tempData[2 * ii] = rawSignals[0][ii];  
             tempData[2 * ii + 1] = 0.0;
        }
        FFTCompute(tempData);
        ConjugateCompute(tempData, rawSignals[0]);

        //O1
        for (int ii = 0; ii < 32; ii++)
        {
            tempData[2 * ii] = rawSignals[1][ii];
            tempData[2 * ii + 1] = 0.0;
        }
        FFTCompute(tempData);
        ConjugateCompute(tempData, rawSignals[1]);

        //O2
        for (int ii = 0; ii < 32; ii++)
        {
            tempData[2 * ii] = rawSignals[2][ii];
            tempData[2 * ii + 1] = 0.0;
        }
        FFTCompute(tempData);
        ConjugateCompute(tempData, rawSignals[2]);

        //AF4
        for (int ii = 0; ii < 32; ii++)
        {
            tempData[2 * ii] = rawSignals[3][ii];
            tempData[2 * ii + 1] = 0.0;
        }
        FFTCompute(tempData);
        ConjugateCompute(tempData, rawSignals[3]);
    }

    private void ConjugateCompute(double[] inputData, double[] outputData)
    {
        for (int ii = 0; ii < 32; ii++)
        {
            outputData[ii] = inputData[ii * 2] * inputData[ii * 2] + inputData[ii * 2 + 1] * inputData[ii * 2 + 1];
        }
    }

    private int CalAttensionLevel(float attentionScores)
    {
        if (attentionScores < thresholdR)
        {
            return 1;
        }
        else if (attentionScores < threshold && attentionScores > thresholdR)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }

    
    private void FFTCompute(double[] inputData)
    {
        /* temp vector, used many times, */
        double[] computeTemp;
        computeTemp = new double[64];

        /* constants for FFT algorithm */
        double[] fftConstant;
        fftConstant = new double[34]{
                1.0,      0.0, 0.980785, 0.19509,
                0.92388,  0.382683,  0.83147,  0.55557,
                0.707107, 0.707107,  0.55557,  0.83147,
                0.382683, 0.92388,   0.19509,  0.980785,
                0.0,      1.0,      -0.19509,  0.980785,
               -0.382683, 0.92388,  -0.55557,  0.83147,
               -0.707107, 0.707107, -0.83147,  0.55557,
               -0.92388,  0.382683, -0.980785, 0.19509,
               -1.0,      0.0};


        double Tre, Tim;
        int ii, jj, preJ, l, target;

        target = 16;
        l = 1;
        while (true)
        {
            preJ = 0;
            jj = l;
            ii = 0;
            while (true)
            {
                while (true)
                {
                    /* computeTemp[ii+preJ] = inputData[ii] + inputData[m+ii]; complex */
                    computeTemp[2 * (ii + preJ)] = inputData[2 * ii] + inputData[2 * (target + ii)];
                    computeTemp[2 * (ii + preJ) + 1] = inputData[2 * ii + 1] + inputData[2 * (target + ii) + 1];

                    /* computeTemp[ii+jj] = fftConstant[preJ] * (inputData[ii] - inputData[m+ii]); complex */
                    Tre = inputData[2 * ii] - inputData[2 * (target + ii)];
                    Tim = inputData[2 * ii + 1] - inputData[2 * (target + ii) + 1];
                    computeTemp[2 * (ii + jj)] = fftConstant[2 * preJ] * Tre - fftConstant[2 * preJ + 1] * Tim;
                    computeTemp[2 * (ii + jj) + 1] = fftConstant[2 * preJ] * Tim + fftConstant[2 * preJ + 1] * Tre;
                    ii++;
                    if (ii >= jj) break;
                }

                preJ = jj;
                jj = preJ + l;
                if (jj > target) break;

            }

            l = l + l;

            if (l > target)
            {
                for (ii = 0; ii < 4 * target; ii++)
                    inputData[ii] = computeTemp[ii]; // odd power finish

                return;
            }

            /* work back other way without copying */
            preJ = 0;
            jj = l;
            ii = 0;

            while (true)
            {
                while (true)
                {
                    /* inputData[ii+preJ] = computeTemp[ii] + computeTemp[m+ii]; complex */
                    inputData[2 * (ii + preJ)] = computeTemp[2 * ii] + computeTemp[2 * (target + ii)];
                    inputData[2 * (ii + preJ) + 1] = computeTemp[2 * ii + 1] + computeTemp[2 * (target + ii) + 1];

                    /* inputData[ii+jj] = fftConstant[preJ] * (computeTemp[ii] - computeTemp[m+ii]); complex */
                    Tre = computeTemp[2 * ii] - computeTemp[2 * (target + ii)];
                    Tim = computeTemp[2 * ii + 1] - computeTemp[2 * (target + ii) + 1];
                    inputData[2 * (ii + jj)] = fftConstant[2 * preJ] * Tre - fftConstant[2 * preJ + 1] * Tim;
                    inputData[2 * (ii + jj) + 1] = fftConstant[2 * preJ] * Tim + fftConstant[2 * preJ + 1] * Tre;
                    ii++;
                    if (ii >= jj) break;
                }

                preJ = jj;
                jj = preJ + l;
                if (jj > target) break;
            }

            l = l + l;
            if (l > target) break; // result is in inputData
        }
    }

}
