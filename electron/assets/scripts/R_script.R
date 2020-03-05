##################################
### R Code For File Processing ###
### Daniel Safai 20/02/2020    ###
##################################

### ROBUST FILE PROCESSING
### Version 3.1
### Note: Script for processing standard R RDS-files (.rds, .RDS, .RData) and statfiles e.g. .sav, .dta etc.
### Imports base formats from spss and stata if any

### Fixes: 
### ASTA paths

# Clear environment
rm(list = ls())

# Import libraries
library(haven)

################# Utility functions ################# 

# Function for convenient quoting of strings
f_quote <- function(x){
  return(paste0("'", x, "'"))
}

# Formatting data frame for output
f_df_format <- function(df, name){
  
  df <- rbind(name, df)
  
  colnames(df) <- 'COL'
  rownames(df) <- c()
  
  return(df)
}

f_process <- function(df, f_name){
  
  # Create temporary output data frame
  df_csv <- df
  
  # Loop over columns to quote factors and characters - except NA values
  for (col in colnames(df_csv)){
    
    if (class(df_csv[[col]])[[1]] %in% c("factor","character")){
      
      df_csv[[col]] <- as.character(df_csv[[col]])
      
      # Identify quote in text and prefix with quote
      df_csv[[col]] <- gsub("\"", "\"\"", df_csv[[col]])
      
      # Paste quotes around text if not NA
      df_csv[[col]][!is.na(df_csv[[col]])] <- paste0('"', df_csv[[col]][!is.na(df_csv[[col]])], '"')
    }
  }
  
  # Output .csv file with ";" as seperator and UTF-encoding
  write.table(df_csv,
              file      = paste0(f_name, ".csv"),
              sep       = ";",
              row.names = FALSE,
              col.names = TRUE,
              quote     = FALSE,
              na        = "",
              fileEncoding = "UTF-8")
  
  # Delete output data frame
  rm(df_csv)
  
  ################# Create "VARIABEL" ################# 
  
  # Initialize data frame for output
  df_variabel <- data.frame(stringsAsFactors = FALSE)
  
  # Determine format
  base_fmt_list <- sapply(df, function(x) names(attributes(x))[grepl("format.",names(attributes(x)))])
  if ("format.spss" %in% base_fmt_list){
    fmt <- "spss"
  } else if ("format.stata" %in% base_fmt_list){
    fmt <- "stata"
  } else if ("format.sas" %in% base_fmt_list){
    fmt <- "sas"
  } else {
    fmt <- "other"
  }
  
  # Loop over each column and extract necessary information
  for (col in colnames(df)){
    
    col_class_i <- class(df[[col]])[[1]]
    col_type_i  <- typeof(df[[col]])[[1]]
    
    # 1) Handle formats for non-statfiles and sas
    if (fmt=="other" || fmt=="sas"){
      if (col_class_i=="Date") {
        variabel_i <- paste(col, "date")
        
      } else if (col_class_i=="hms"){
        variabel_i <- paste(col, "time")
        
      } else if (col_class_i=="times"){
        variabel_i <- paste(col, "time")
        
      }else if (col_class_i=="POSIXct"){
        variabel_i <- paste(col, "datetime")
        
      } else if (col_class_i=="double"){
        variabel_i <- paste(col, "decimal")
        
      } else if (col_class_i=="numeric"){
        variabel_i <- paste(col, "decimal")
        
      } else if (col_class_i=="integer"){
        variabel_i <- paste(col, "int")
        
      } else {
        variabel_i <- paste(col, "string")
      }
    }
    
    # 2) Handle formats for spss/stata files
    var_fmt_list <- c("%tdCCYY-NN-DD",
                      "%tcHH:MM:SS",
                      "%tcCCYY-NN-DD!THH:MM:SS",
                      "%tcCCYY-NN-DD!THH:MM:SS.s",
                      "%tcCCYY-NN-DD!THH:MM:SS.ss",
                      "%tcCCYY-NN-DD!THH:MM:SS.sss",
                      "h:m:s")
    if (fmt=="spss" || fmt=="stata"){
      fmt_special <- attributes(df[[col]])[grepl("format.", 
                                                 names(attributes(df[[col]])))]
      
      if (fmt_special %in% var_fmt_list){
        variabel_i <- paste(col, fmt_special)
        
      } else {
        variabel_i <- paste(col, tolower(fmt_special))
      }
    }
    
    # Handle "kodeliste" reference for converted variables
    
    if (col_class_i=="haven_labelled"){
      # Note: Make assumption that all converted i.e. haven_labelled variables are decimals (except stata/spss)
      # Thus, no dates, strings etc. have a description
      if (fmt=="sas"){
        variabel_i <- paste(col, "decimal", paste0(attributes(df[[col]])$format.sas,"."))
        
      } else if (fmt=="spss" || fmt=="stata"){
        fmt_special <- attributes(df[[col]])[grepl("format", 
                                                   names(attributes(df[[col]])))]
        variabel_i <- paste(col, 
                            tolower(fmt_special), 
                            paste0(col,'.'))
        
      } else {
        # Note: If non-stat format, but haven_labelled, we assume decimal
        variabel_i <- paste(col, "decimal", paste0(col,'.'))
      }
    } else if (!is.null(attributes(df[[col]])$labels)){
      # Note: If "labels" are attached to column, but not haven_labelled class
      if (fmt=="sas"){
        
        col_class_i = attributes(df[[col]])$class[[1]]
        
        if (col_class_i=="Date") {
          variabel_i <- paste(col, "date", paste0(attributes(df[[col]])$format.sas,"."))
          
        } else if (col_class_i=="hms"){
          variabel_i <- paste(col, "time", paste0(attributes(df[[col]])$format.sas,"."))
          
        } else if (col_class_i=="times"){
          variabel_i <- paste(col, "time", paste0(attributes(df[[col]])$format.sas,"."))
          
        }else if (col_class_i=="POSIXct"){
          variabel_i <- paste(col, "datetime", paste0(attributes(df[[col]])$format.sas,"."))
          
        } else if (col_class_i=="double"){
          variabel_i <- paste(col, "decimal", paste0(attributes(df[[col]])$format.sas,"."))
          
        } else if (col_class_i=="numeric"){
          variabel_i <- paste(col, "decimal", paste0(attributes(df[[col]])$format.sas,"."))
          
        } else if (col_class_i=="integer"){
          variabel_i <- paste(col, "int", paste0(attributes(df[[col]])$format.sas,"."))
          
        } else {
          variabel_i <- paste(col, "string", paste0(attributes(df[[col]])$format.sas,"."))
        }
        
      } else {
        
        variabel_i <- paste(col, "decimal", paste0(col,'.'))
      }
      
    }
    
    # Concatenate result to data frame
    df_variabel <- rbind(df_variabel, data.frame(variabel_i, stringsAsFactors = FALSE))
    
  }
  
  # If variables exist proces and write file
  if (nrow(df_variabel)>0){
    
    # Prepare data frame for output
    df_variabel <- f_df_format(df_variabel, 'VARIABEL')
    
    # Save data frame
    write.table(df_variabel,
                file      = paste0(f_name, "_", "VARIABEL.txt"),
                row.names = FALSE,
                quote     = FALSE,
                col.names = FALSE)
  }
  
  ################# Create "VARIABELBESKRIVELSE" ################# 
  
  # Extract variable description
  col_lbl_list <- sapply(df, function(x) attributes(x)$label)
  
  # Remove variables with no label available
  col_lbl_list <- col_lbl_list[!sapply(col_lbl_list, is.null)]
  
  # Convert to data frame
  df_description <- as.data.frame(col_lbl_list, 
                                  stringsAsFactors = FALSE, 
                                  row.names = names(col_lbl_list))
  
  # If descriptions exist proces and write file
  if (nrow(df_description)>0){
    
    # Paste description and column name in single column - if description exist
    if (ncol(df_description)>0){
      df_description[,1] <- paste(rownames(df_description), f_quote(df_description[,1]))
    }
    
    # Prepare data frame for output
    df_description <- f_df_format(df_description, 'VARIABELBESKRIVELSE')
    
    # Save data frame
    write.table(df_description,
                file      = paste0(f_name, "_", "VARIABELBESKRIVELSE.txt"),
                row.names = FALSE,
                quote     = FALSE,
                col.names = FALSE)
  }
  
  ################# Create "KODELISTE" ################# 
  
  # Initialize data frame
  df_kodeliste <- data.frame(stringsAsFactors = FALSE)
  
  # Determine format
  base_fmt_list <- sapply(df, function(x) names(attributes(x))[grepl("format.",names(attributes(x)))])
  if ("format.spss" %in% base_fmt_list){
    fmt <- "spss"
  } else if ("format.stata" %in% base_fmt_list){
    fmt <- "stata"
  } else if ("format.sas" %in% base_fmt_list){
    fmt <- "sas"
  } else {
    fmt <- "other"
  }
  
  # Loop over each column and extract kodeliste
  for (col in colnames(df)){
    
    col_lbls_i  <- attributes(df[[col]])$labels
    col_vals_i  <- names(col_lbls_i)
    col_names_i <- unname(col_lbls_i)
    
    if (!is.null(col_lbls_i)){
      if (fmt=="sas"){
        
        # If variable already exists in kodeliste continue
        if (attributes(df[[col]])$format.sas %in% df_kodeliste[,1]){
          next
        }
        
        kodeliste_i <- c(attributes(df[[col]])$format.sas,
                         paste(f_quote(col_names_i[!is.na(col_lbls_i)]), 
                               f_quote(col_vals_i[!is.na(col_lbls_i)])))
        
      } else {
        kodeliste_i <- c(col,
                         paste(f_quote(col_names_i[!is.na(col_lbls_i)]), 
                               f_quote(col_vals_i[!is.na(col_lbls_i)])))
      }
      
    } else {
      kodeliste_i <- NULL
    }
    
    df_kodeliste <- rbind(df_kodeliste, data.frame(kodeliste_i, stringsAsFactors = FALSE))
  }
  
  # If any kodeliste input exist proces and write file
  if (nrow(df_kodeliste)>0){
    
    # Prepare data frame for output
    df_kodeliste <- f_df_format(df_kodeliste, 'KODELISTE')
    
    # Save data frame
    write.table(df_kodeliste,
                file      = paste0(f_name, "_", "KODELISTE.txt"),
                row.names = FALSE,
                quote     = FALSE,
                col.names = FALSE)
  }
  
  # Report when done
  print("Data processing succesfully completed")
  
}

# ASTA input paths
input_path <- "C:/Users/lokalAdmin/Desktop/TestSuite/AKEtestning/Accepttest_R-script/02_spss_to_rds_testfil" # "{1}"
output_path <- "C:/Users/lokalAdmin/Desktop/TestSuite/AKEtestning/Accepttest_R-script" # "{3}"
file_name <- "spss12345" # "{2}" Note: input skal v�re som strings -> paste0('"', {2}, '"')

# Set working dir - Also the path for saving files
setwd(output_path)

# Load file
file <- tryCatch(readRDS(paste0(input_path, "/", file_name, ".rds")),
                 warning = function(x) readRDS(paste0(input_path, "/", file_name, ".Rds")))

# Proces file
f_process(file, file_name)


