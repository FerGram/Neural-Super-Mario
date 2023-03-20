-----------------Welcome to the Platformer Experience Dataset-----------------
Each folder is for one participant. The data in each folder is organised as follows (where 'Number' is the session number (a pair of two levels) and 'Letter' indicates the order of the level in a pair (where A is the first level in a pair and B is the second):

1. Number_Letter.avi: contains the video recording 

2. Number_Letter.csv: contains the gameplay data associated with each video file. The first column is the timestamp while the second is an action performed by the player. Each action is given a unique code as follows: 

ARMORED_TURTLE_KILLSTOMP = 311JUMP_FLOWER_KILLSTOMP =411CANNON_BALL_KILLSTOMP = 511CHOMP_FLOWER_KILLSTOMP = 611RED_TURTLE_UNLEASHED = 021GREEN_TURTLE_UNLEASHED = 121GOOMPA_UNLEASHED = 221ARMORED_TURTLE_UNLEASHED = 321JUMP_FLOWER_UNLEASHED =421CANNON_BALL_UNLEASHED = 521CHOMP_FLOWER_UNLEASHED = 621LITTLE_START = 020LARGE_START = 120FIRE_START = 220LITTLE_END = 021LARGE_END = 121FIRE_END = 221JUMP_START = 030DUCK_START = 130RUN_START = 230LEFT_MOVE_START = 330RIGHT_MOVE_START = 430JUMP_END = 031DUCK_END = 131RUN_END = 231LEFT_MOVE_END = 331RIGHT_MOVE_END = 431

3. _Dem.csv: contains the demographics information in the following order: playedBefore, timePlaying,play,age, sex. The questionnaire presented, their corresponding possible answers and the associated values are as follows: 

playedBefore: "Have you played Super Mario before?"
	yes: 0
	No: 1

timePlaying: "How much time do you spend playing videogames?"
	0-2 hours per week: 0
	2-5 hours per week: 1
	5-10 hours per week:2
	10+ hours per week: 3

Play: "Do You Play VideoGames?"
	Yes: 0
	No: 1

sex:
	male: 0
	fema

4: _Pref.csv: the preference and ranking values given to all game played in the following order:levelAEngagementRanking,levelAFrustrationRanking,levelAChallengeRanking,levelBEngagenemtRanking,levelBFrustrationRanking,levelBChallengeRanking,EngagementPreferences,frustrationPreferences,challangePreferences. The values presented are assigned as follows:

Ranking Values:
0:Extremely
1:Fairly
2:Moderately
3:Slightly
4:Not at all

Preference Values:
0: A
1: B
2: EQUAL
3: NEITHER
-1:invalid

     