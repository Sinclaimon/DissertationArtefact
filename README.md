# Interactive tree evolution - Computing artefact ### 


This is an interactive evolution of 2D trees for my computing artefact, aloing with the data analysis R code that was conducted for my dissertation. 
If you want to try out the evolution, download the source code and run it as a unity project. The artefact was made for Unity v2021.3.4f1.


The main functionality of the code is in GrammaticalEvolution.cs, along with the main Update method. If you would like to change the base lsystems or tinker with the rules / alphabet / degree settings, go to SimpleTree.cs to method TreeSetup(), or create a new prefab variant of the Simple Tree prefab and assign it to the treeObject in Grammatical Evolution game object. 
### Note that too many Lsystem iterations may freeze Unity.


## Current main functionality of the Unity project:
1. Click on generated trees you like for them to have a major part in the next evolution's visuals, if you want to pick more than 3 trees per generation or the number of generations, change it in Grammatical Evolution game object in the main scene, under requiredPicks and requiredGenerations variables.
2. Evolving trees: Current selection process is based on the interactive aspect, mutation and crossover genetic operators implemented.
3. Save Population data to a json file, this happens automatically once the required number of generations is met, location of the saved file can be found in the debug console.
4. Load Population data from earlier version of the artefact and update their marking results.
5. Resave the loaded population data from earlier version with updated marking, by pressing F once the "Game" is running and entering the path to the folder with the population data.
6. Load Population data from already generated population data files and display the top 10 best trees based on the marking, by pressing B once the "Game" is running and entering the path to the folder with the population data.


## Additional functionality of the Unity project.
1. Evolve population at any given time by pressing G.
2. Reset the current picked trees by pressing R.
3. Redraw the current popupation by pressing D.


## Main functionality of the RStudio project
1. Load a excel sheet from the aesthetics questionnaire that was used for collecting user scores for the generated trees.
2. Visualize this excel data in multiple ways.
3. Attempt to load a folder of JSON files and extract the marking results from them (currently not working fully) 


## Branches and their use: 
1. main - used to developing the Unity project, final commenting and initial prototyping - currenly the most up-to-date branch
2. R-data-analysis - used for developing the R code for data analysis, last merge to main on 1st of April 2023
3. dev - used for developing the Unity project, along with multiple sub-branches for refactoring, last merge to main on 11th of March 2023
4. tree-respawn-refactor - used for refactoring tree spawning and additional population related functionality, including updating genetic functions, last merge to dev on 10th of March 2023
5. data-export-to-file - used for adding the functionality to export generated populations to a JSON file, last merge to dev 10th of March 2023




[Link to the build that was used for data collection](https://falmouthac-my.sharepoint.com/:u:/g/personal/sm255559_falmouth_ac_uk/EdoG1K3VRH5GvdUAiYAGFGIBLR1ICQsVLQ5OmcGlspeEug?e=tAA8lz) 
