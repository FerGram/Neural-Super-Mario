This project focus on training an ANN (Artificial Neural Network) to be able to predict the level of engagement, 
challenge and frustration of a player when playing a level of a Super Mario Bros-like game (2D platformer). 

The data to train this ANN has been downloaded from https://ped.institutedigitalgames.com/. This database 
has captured data of multiple people who have played different levels of a similar 2D platformer and has recorded
data like:
- Information about the player (age, sex, if he/she has played before, etc.)
- Action and time that action was performed
- Eye tracking and facial expression data
- Level rating (frustration, challenge, engagement between levels)

Eye tracking and facial expression data has not been used for training the ANN.

There has been multiple layers of data processing to get to three single CSV files we can train the ANN with (one
per each output). The main script that has scanned through all files and got + transformed the desired data is 
the DataProcessor.py script. However, some manual modifications have been done on the final CSV data files as 
developing the code was not worth the effort. The DataProcessor.py script is of one use only and it's has not 
been tidied up for reading.

Each row of any of the CSV files is a combination of:
1. The demographic data of the player (5 columns)
2. The amount of jumps, left move and right move actions for run 1 + the release of those actions + time of run 1 (7 columns)
3. The amount of jumps, left move and right move actions for run 2 + the release of those actions + time of run 2 (7 columns)
3. The amount of jumps, left move and right move actions for run 3 + the release of those actions + time of run 3 (7 columns)
4. The user rating of engagement, frustration or challenge depending of the file(1 column). This is our desired output.

With the data processed, the DataTrainer.py constructs an ANN model in Pytorch that learns to predict each rating (not all
at once, but we export one model per rating so we can have them separate). This were the training hyperparameters used:
- Challenge: 	4000  epochs, ADAM optimizer, 0.000001 learning rate
- Engagement: 	10000 epochs, ADAM optimizer, 0.000001 learning rate
- Frustration: 	10000 epochs, ADAM optimizer, 0.00001 learning rate

The models accuracies achieved are not high, but sufficient enough to move on to Unity and have OK results (around 50% accuracy).

This project uses as a starting point the previous submission of the "content generation based on user modalization" task.
The Barracuda package has been imported to handle the Neural Network calls and predictions. What have been the three predictions
been used to:
- Challenge: for Enemy increase/decrease in the adaptativeFitness() function
- Engagement: for General level mutation
- Frustration: for Gap increase/decrease in the adaptativeFitness() function