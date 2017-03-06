using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using Emotiv;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using SampEn;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
//using Microsoft.VisualBasic.FileIO;
//using computeAttention;


public class EEGLogger {

    //[DllImport("edk")]
    //public static extern void EDK_cleanup();

    public int readCount;

    EmoEngine engine;

    //(assemblies);
    public static bool USER_ADDED { get { return (userID > -1); } }
    private static int instanceCount = 0;
    private static int userID = -1;
    private static string root_path = Directory.GetParent(Environment.CurrentDirectory).ToString();
    private static string trainR_root_path = root_path + "\\testdata\\Train\\";
    private static string trainF_root_path = root_path + "\\testdata\\Train\\";
    private static string game_root_path = root_path + "\\testdata\\Game\\";
    //private string trainRSignalFile = trainR_root_path + "Signals_" + DateTime.Now.ToString("ddMMyyyy") + ".csv";
    private static string trainRAttentFile = trainR_root_path + "Attent_" + DateTime.Now.ToString("ddMMyyyy") + ".csv";
    //private string trainFSignalFile = trainR_root_path + "Signals_" + DateTime.Now.ToString("ddMMyyyy") + ".csv";
    private static string trainFAttentFile = trainR_root_path + "Attent_" + DateTime.Now.ToString("ddMMyyyy") + ".csv";
    private static string gameSignalFile = game_root_path + "Signals_" + DateTime.Now.ToString("ddMMyyyy") + ".csv";
    private static string gameAttentFile = game_root_path + "Attent_" + DateTime.Now.ToString("ddMMyyyy") + ".csv";
    private static int bufferSizeLimit = 64;
    private double[][] rawSignals = new double[4][];

    public static bool ThresholdRSet;
    public static bool ThresholdSet;
    private static double thresholdR;
    private static double threshold;
    private double BTconcentrationLevel;
    private static float startTime;
    private static bool testMode = true;
    // private static EEGLogger logger;


    // Use this for initialization
    public EEGLogger() {

        //Debug.Log("EEGLogger!");
        engine = EmoEngine.Instance;

        //Debug.Log(engine);
        threshold = 0;
        thresholdR = 0;
        readCount = 0;
        ThresholdSet = false;
        ThresholdRSet = false;

        for (int i = 0; i < 4; i++)
        {
            rawSignals[i] = new double[bufferSizeLimit];
        }

        try {
            engine.UserAdded += new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);
            engine.UserRemoved += new EmoEngine.UserRemovedEventHandler(engine_UserRemoved_Event);
            engine.Connect();

            //writeHeader();
            //EmoEngine.errorHandler(EdkDll.EE_EngineConnect("Emotiv Systems-5"));
            //Debug.Log("engine:"+engine);
        }
        catch (Exception e) {
            Debug.Log(e);
            Debug.Log("Unable to connect!");
        }

        //   instanceCount++;
    }

    void writeHeader() {

        TextWriter file = new StreamWriter(gameSignalFile, true);
        Debug.Log(gameSignalFile);
        //WriteHeader;
        file.WriteLine("COUNTER, INTERPOLATED,RAW_CQ,AF3,F7,F3,FC5,T7,P7,O1,O2,P8,T8,FC6,F4,F8,AF4,GYROX,GYROY,TIMESTAMP,ES_TIMESTAMP,FUNC_ID,FUNC_VALUE,MARKER,SYNC_SIGNAL");
        file.Close();

    }

    void engine_UserAdded_Event(object sender, EmoEngineEventArgs e) {
        //Thread t = Thread.CurrentThread;
        //Debug.Log(t.Name + t.ThreadState);
        // t.Suspend();
        //t.Yield();// Suspend();
        Debug.Log("Dongle Plugged!!!!!!!");
        userID = (int)e.userId;

        try
        {

            int errorCode = EdkDll.EE_DataAcquisitionEnable((uint)userID, true);
            //engine.DataAcquisitionEnable((uint)userID, true);
            EmoEngine.errorHandler((Int32)errorCode);
            Debug.Log("errorCode = " + errorCode);
            //enable the data aquisition for this user

            //[DllImport("edk.dll", EntryPoint = "EE_DataAcquisitionEnable")]
            // static extern Int32 Unmanaged_EE_DataAcquisitionEnable(UInt32 userId, Boolean enable);
        }
        catch (Exception exp)
        {
            Debug.Log("error");
            Debug.Log(exp);
        }


        try {

            bool result;
            int err2 = EdkDll.EE_DataAcquisitionIsEnabled((uint)userID, out result);
            Debug.Log(err2);
            EmoEngine.errorHandler((Int32)err2);
        } catch (Exception exp) {
            Debug.Log("error");
            Debug.Log(exp);
        }

        EmoState emoState = new EmoState();
        EdkDll.EE_EEG_ContactQuality_t[] qualities = emoState.GetContactQualityFromAllChannels();

        int i;
        for (i = 0; i < qualities.Length; i++) {
            Debug.Log(qualities[i]);
        }
        // EE_DataAcquisitionIsEnabled(uint userId, out bool pEnableOut);
        //engine.DataAcquisitionEnable((uint)userID, true);

        uint sampleRate = engine.DataGetSamplingRate((uint)userID);
        Debug.Log(sampleRate);
        //ask for up to 1s of buffered data
        engine.EE_DataSetBufferSizeInSec(1);
        int num = (int)engine.EngineGetNumUser();
        Debug.Log("user number = " + num);

    }

    void engine_UserRemoved_Event(object sender, EmoEngineEventArgs e)
    {
        Debug.Log("Dongle Removed!");
        userID = -1;
    }

    public static void WaitingForUser()
    {
        Debug.Log("Create the waiting thread!");
        EEGLogger logger = new EEGLogger();
        while (userID == -1)
        {
            Debug.Log("waiting thread!");
            Thread.Sleep(1000);
        }
        SceneManager.LoadScene("MainMenu");
    }


    double[] AF3 = new double[320];
    double[] O1 = new double[320];
    double[] O2 = new double[320];
    double[] AF4 = new double[320];

    public static void SetThresholdR() {
        Debug.Log("Relax Training Thread Created!");

        EEGLogger logger = new EEGLogger();
        
        TextWriter file;
        float attentionSc = -1;
        string dataLocation = EEGLogger.trainRAttentFile;
        file = new StreamWriter(dataLocation, true);

        //set up threshold for attention
        //if (testMode){
        //logger.setFakeThresholds();
        //EEGLogger.

        // EEGLogger.
        //ThresholdSet = true;
        //}
        //else
        //{

        if (testMode)
        {
            string fileName = root_path + "\\testdata\\signals_2.csv";
            Debug.Log(fileName);

            
            //read line by line
            try
            {
                var reader = new StreamReader(File.OpenRead(fileName));
                var line = reader.ReadLine();
                var values = line.Split(',');
                var temp = values.Select(x => Double.Parse(x)).OrderBy(x => x).ToArray();
                Array.Copy(temp, 0, logger.AF3, 0, 320);
                Debug.Log(logger.AF3);
                //Array.Copy(values, logger.AF3, 320);

                //line = reader.ReadLine();
                //values = line.Split(',');
                //Array.Copy(values, logger.O1, 320);

                //line = reader.ReadLine();
                //values = line.Split(',');
                //Array.Copy(values, logger.O2, 320);

                //line = reader.ReadLine();
                //values = line.Split(',');
                //Array.Copy(values, logger.AF4, 320);
            }
            catch (Exception e) {
                Debug.Log(e);
            }
           // var reader = new StreamReader(File.OpenRead(fileName));


            //compute attentionScores
            Debug.Log("EEG RUNNING!");
            int counter = 0;
            for (counter = 0; counter < 5; counter++)
            {
                Debug.Log(counter);
                try
                {
                    double attenScore = logger.FakeRun(counter);
                }
                catch(Exception e) {
                    Debug.Log(e);
                }
                
                counter++;
                Thread.Sleep(250);
            }
        }
        else
        {
            int counter;

            if (userID > -1)
            {
                for (counter = 0; counter < 127; counter++)
                {
                    if (logger.Run())
                    {
                        logger.TimeToFrq32();
                        attentionSc += logger.CalculateAttention();
                        file.WriteLine(attentionSc);
                    }
                    Thread.Sleep(250);
                }
                file.Close();
            }
            else
            {
                Debug.Log("No user! Can not compute thresholdR!");
            }
        }

       

        //    if (userID > -1)
        //    {

        //        if (logger.Run())
        //        {
        //            file = new StreamWriter(dataLocation, true);
        //            logger.readCount++;
        //            logger.TimeToFrq32();
        //            attentionSc = (float)logger.CalculateAttention();
        //            attentionLv = logger.CalAttensionLevel(attentionSc); ;
        //            Player.attentionScore = attentionSc;
        //            Player.attentionLvl = attentionLv;

        //            file.WriteLine(attentionSc + ",  " + attentionLv);
        //            file.Close();
        //            //TODO: Update UI display
        //        }
        //    }

        EEGLogger.thresholdR = attentionSc / 128;
        Console.WriteLine("thresholdR = " + EEGLogger.thresholdR);

        //}
        file.Close();
        ThresholdRSet = true;
        Debug.Log("thresholdR set!");
        //GameObject relaxController = GetComponent<RelaxTraining>();
    
    //while (true)
    //{
    //    Thread.Sleep(250);

    //    //if (testMode){
    //    file = new StreamWriter(dataLocation, true);
    //    logger.readCount++;
    //    attentionSc = (float)logger.generateFakeAttention(); //0-100
    //    attentionLv = logger.CalAttensionLevel(attentionSc); ;
    //    Player.attentionScore = attentionSc;
    //    Player.attentionLvl = attentionLv;
    //    Player.UpdateHorizontalSpeed();
    //    file.WriteLine(attentionSc + ",  " + attentionLv);
    //    file.Close();

    //    //}else{
    //    if (userID > -1)
    //    {

    //        if (logger.Run())
    //        {
    //            file = new StreamWriter(dataLocation, true);
    //            logger.readCount++;
    //            logger.TimeToFrq32();
    //            attentionSc = (float)logger.CalculateAttention();
    //            attentionLv = logger.CalAttensionLevel(attentionSc); ;
    //            Player.attentionScore = attentionSc;
    //            Player.attentionLvl = attentionLv;

    //            file.WriteLine(attentionSc + ",  " + attentionLv);
    //            file.Close();
    //            //TODO: Update UI display
    //        }
    //    }
    //}

    //   }

}
    // private bool Run() { }
    public double FakeRun(int readcount) {
        //bool getSamples = false;

        
        //if (testMode) {
        double[] eeg = new double[32];
        Array.Copy(AF4, 32 * (readcount % 10), eeg, 0, 32);

        try {

            MWArray[] sampResults = null;
            MWArray attenScore = null;
            MWNumericArray a = new MWNumericArray(1, 32, (double[])eeg);
            //SampEnClass sampClass = new SampEnClass();//(float)logger.generateFakeAttention(); //0-100
            //double std_value = StandDeviation(eeg);
            //sampResults = sampClass.SampEn(2, 0.25 * std_value, a);
            //attenScore = sampResults[0];
            //Debug.Log(attenScore);
        } catch (Exception e) { Debug.Log(e); }
       

        return 0;
    }



    public static void OnRetrieveData() {

        Debug.Log("OnRetrieveData Thread Created!");

        EEGLogger logger = new EEGLogger();

        TextWriter file;
        float attentionSc = -1;
        int attentionLv = -1;

        //string root_path= Environment.CurrentDirectory;
        //string path = Directory.GetParent(root_path).ToString();//Directory.GetCurrentDirectory()//.Parent
        string dataLocation = EEGLogger.gameAttentFile;//path + "\\testdata\\Game\\" + DateTime.Now.ToString(@"yyyyddMM_HHmm") + ".csv";

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

        file = new StreamWriter(dataLocation, true);
        //if (testMode) {
       

        double[] AF3 = new double[320];
        double[] O1 = new double[320];
        double[] O2 = new double[320];
        double[] AF4 = new double[320];

       

        //}//end if



        int readcount = 0;

        while (true)
        {
            Debug.Log("EEG RUNNING!");

            //if (testMode) {
            double[] eeg = new double[32];
            Array.Copy(AF4, 32 * (readcount % 10), eeg, 0, 32); //Array.Copy(data, index, result, 0, length);;
                                                                //attention_sc += SampEn(2, 0.25 * std(eeg), eeg);
                                                                //end
            MWNumericArray a = new MWNumericArray(1, 32, (double[])eeg);
            SampEnClass sampClass = new SampEnClass();//(float)logger.generateFakeAttention(); //0-100
            double std_value = logger.StandDeviation(eeg);
            //sampResults = sampClass.SampEn(2, 0.25 * std_value, a);
            //attenScore = sampResults[0];
            //Debug.Log(attenScore);

                //attentionLv = logger.CalAttensionLevel(attentionSc); ;


            Player.attentionScore = attentionSc;
            Player.attentionLvl = attentionLv;
            Player.UpdateHorizontalSpeed();
            file.WriteLine(attentionSc + ",  " + attentionLv);
            file.Close();

            //}
            //else
            //{
            //    Debug.Log("Real mode: Continue!");
            //    if (userID > -1)
            //    {

            //        if (logger.Run())
            //        {
            //            file = new StreamWriter(dataLocation, true);
            //            logger.readCount++;
            //            logger.TimeToFrq32();
            //            attentionSc = (float)logger.CalculateAttention();
            //            attentionLv = logger.CalAttensionLevel(attentionSc); ;
            //            Player.attentionScore = attentionSc;
            //            Player.attentionLvl = attentionLv;

            //            file.WriteLine(attentionSc + ",  " + attentionLv);
            //            file.Close();
            //            TODO: Update UI display
            //        }
            //}
            //}//end if/else

            Thread.Sleep(250);
        }
    }//end while

      
    

    private double StandDeviation(double[] array)
    {
        int arrayLength = array.Length;
        double sum = 0;

        for (int i = 0; i < arrayLength; i++)
        {
            sum += array[i];
        }
        double average = sum / arrayLength;
        double sumOfSquaresOfDifferences = 0;
        double dif;
        for (int i = 0; i < array.Length; i++)
        {
            dif = array[i] - average;
            sumOfSquaresOfDifferences += dif * dif;
        }

        double sd = Math.Sqrt(sumOfSquaresOfDifferences / arrayLength);

        return sd;
    }


    /*1)retrieve data signals
     * 2)record signal into files
     3)Compute attentionScore*/
    private bool Run() {

        Debug.Log("Current Thread: "+Thread.CurrentThread.Name+"  in Run()");
        // Handle any waiting events
        engine.ProcessEvents();
     
        bool result = engine.IsDataAcquisitionEnabled((uint)userID);

        Debug.Log(result);
        // If the user has not yet connected, do not proceed
        if ((int)userID == -1)
        {
            var message = "Please connect your headset first!";
            //var title = "Headset Not Found";
            Debug.Log(message);
            return false;
        }
        else if (!result) {
            Debug.Log("User " + userID + " can not acquire data from Emotiv!");
        }
        Dictionary<EdkDll.EE_DataChannel_t, double[]> data = engine.GetData((uint)userID);

        if (data == null)
        {
            Debug.Log("Data = null");
            return false;
        }

        int _bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;
        string dataLocation = EEGLogger.gameSignalFile;
        TextWriter file = new StreamWriter(dataLocation, true);
        // Write the data to a file
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
    private void setFakeThresholds() {

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
                rawSignals[1][7] + rawSignals[1][8] + rawSignals[1][9] + rawSignals[1][10] +
                rawSignals[2][7] + rawSignals[2][8] + rawSignals[2][9] + rawSignals[2][10] +
                rawSignals[3][7] + rawSignals[3][8] + rawSignals[3][9] + rawSignals[3][10];

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
