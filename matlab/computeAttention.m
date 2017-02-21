function [attentionScore] = computeAttention(data) 
%%attention score range = [0,100]
 
attentionScore = mean(data);
%%y = sprintf(attentionScore); 