
# LOAD PACKAGES ###############################################################

library(datasets)
library(jsonlite)
library(readxl)
library(ggplot2)
library(GGally)

# SETUP #######################################################################

dir <- "data_json"
# List all JSON files in the folder
files <- list.files(dir, pattern = "*.json", full.names = TRUE)
# Initialize empty data frame of JSON data
all_json_data <- list()

# LOAD JSON DATA ###################################################################

# Import all JSON files from the given folder

json_data_list <- lapply(files, function(json_file) {
  # Import file
  json_data <- jsonlite::fromJSON(json_file)
  
  # Extract generation number
  generationNumber = json_data[["genNumber"]]
  
  # Check if generationNumber is NULL
  if (is.null(generationNumber)) {
    print("generationNumber data is missing / null")
  }
  
  # Check if lsystemsData field is present
  if (!"lsystemsData" %in% names(json_data)) {
    print("lsystemsData field missing")
  }
  
  # Store lsystemsData sub field
  allLsystemsData <- json_data[["lsystemsData"]]
  
  # Check if allLsystemsData is NULL
  if (is.null(allLsystemsData)) {
    print("lsystemsData data is missing / null")
  }
  
  
  # Extract population ID from file name using regex
  populationID <- gsub(".*/|\\.json", "", json_file)
  
  generation_Trees <- list(populationID = populationID)
  
  print(generation_Trees)
  ## LOAD ONE GENERATION'S DATA ===============================================
  
  # Get data on all the trees in a given generation
  for (i in seq_along(allLsystemsData)) {
    
    # Get data of a specific tree
    tree_data <- allLsystemsData[[i]]
    # Extract the fitness structure
    fitness_values <- tree_data[["fitness"]]
    # Extract all the relevant values into a list of output data frame
    tree_metrics <- list(
      overallFitnessScore = fitness_values[["overallFitness"]],
      positivePhotoTropism = fitness_values[["positivePhototropism"]],
      bilateralSymmetry = fitness_values[["bilateralSymmetry"]],
      branching = fitness_values[["branchingPointsProportion"]],
      pickWeight = tree_data[["finalWeight"]],
      branchCount = tree_data[["finalBranchCount"]],
      treeName = fitness_values[["treeName"]],
      lsystemSentence = tree_data[["sentence"]],
      generation = generationNumber
    )
    #print("tree metrics:")
    #str(tree_metrics)
    
    if (is.null(tree_metrics)) {
      print("tree_metrics data is null")
    }
    
    #str(tree_metrics)
    
    # Add tree metrics to the list of trees in a generation
    generation_Trees <- c(generation_Trees, tree_metrics)
    print(append("generation Trees length after append: ", 
                 length(generation_Trees)))
  }
  str(generation_Trees)
  # Merge tree data with the rest of json data we collected so far
  all_json_data <- c(all_json_data, list(generation_Trees))
  print(append("all_json_data length after append: ", length(all_json_data)))
})

str(all_json_data)

# EXCEL LOADING AND DATA MERGING ##############################################

# Load excel file into a df
q_data <- read_excel("questionnaireAnswers.xlsx")


# CORRELATIONS AND MEANS CALC #################################################

CalcCorr_Means <- function(q_data) {
  # H1 correlation analysis between aesthetics and realism 
  cor_aesthetic_realism_first <- cor.test(q_data$VisualAppealFirstGen, 
                                          q_data$RealismFirstGen,
                                          conf.level = 0.95)
  
  cor_easthetic_realism_last <- cor.test(q_data$VisualAppealLastGen, 
                                         q_data$RealismLastGen,
                                         conf.level = 0.95)
  
  # Print correlations for first and last generations:
  print(cor_aesthetic_realism_first)
  print(cor_easthetic_realism_last)
  
  
  # H2 mean comparison for realism scores
  mean_realism_first <- mean(q_data$RealismFirstGen)
  mean_realism_last <- mean(q_data$RealismLastGen)
  
  # Print mean for first and last realism scores:
  print(mean_realism_first)
  print(mean_realism_last)
  
  # H3 average control score
  mean_control <- mean(q_data$ControlMetric)
  
  print(mean_control)
}


#VISUALIZATION FUNCTION #######################################################

Visualize <- function(q_data) {
  # Scatterplot matrix for all the user scores and corr
  scatterplot_matrix <- ggpairs(q_data, columns = 2:6)
  print(scatterplot_matrix)
  
  # Boxplot realism first gen and realism last gen
  box_plot_realism <- ggplot(q_data) +
    geom_boxplot(aes(x = factor(0), y = RealismFirstGen), fill = "blue") +
    geom_boxplot(aes(x = factor(1), y = RealismLastGen), fill = "red") +
    labs(title = "Boxplot of RealismFirstGen and RealismLastGen",
         x = "Generations",
         y = "Realism")
  
  print(box_plot_realism)
  
  # Bar plot of ControlMetric
  control_metric_plot <- ggplot(q_data, aes(x = factor(ControlMetric))) +
    geom_bar(fill = "orange") +
    labs(title = "Distribution of User control perception score",
         x = "User control perception score",
         y = "Frequency")
  
  print(control_metric_plot)
  
  # Scatterplot for VisualAppealFirstGen vs RealismFirstGen
  scatter_plot_first_gen <- ggplot(q_data, aes(x = VisualAppealFirstGen, 
                                             y = RealismFirstGen)) +
    geom_point(color = "blue") +
    geom_smooth(method = "lm", se = TRUE, color = "red") +
    labs(title = "Scatterplot of VisualAppealFirstGen vs RealismFirstGen",
         x = "Visual Appeal (First Generation)",
         y = "Realism (First Generation)")
  
  print(scatter_plot_first_gen)
  
  # Scatterplot for VisualAppealLastGen vs RealismLastGen
  scatter_plot_last_gen <- ggplot(q_data, aes(x = VisualAppealLastGen, 
                                            y = RealismLastGen)) +
    geom_point(color = "blue") +
    geom_smooth(method = "lm", se = TRUE, color = "red") +
    labs(title = "Scatterplot of VisualAppealLastGen vs RealismLastGen",
         x = "Visual Appeal (Last Generation)",
         y = "Realism (Last Generation)")
  
  print(scatter_plot_last_gen)
}

# FUNCTION CALLS ##############################################################
CalcCorr_Means(q_data)
Visualize(q_data)


