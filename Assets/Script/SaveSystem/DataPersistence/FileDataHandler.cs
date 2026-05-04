using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;


public class FileDataHandler
{
    private string dataDirPath = "";
    private string DataFileName = "";

    public FileDataHandler(string dataDirPath, string DataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.DataFileName = DataFileName;
    }

    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, DataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                //Load the serialized data from the file
                string DataToLoad = "";
                    using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        DataToLoad = reader.ReadToEnd();
                    }
                }
                //Deserealized  the dataa from JSON back into a C# object
                loadedData = JsonUtility.FromJson<GameData>(DataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error Occured when trying to Load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(GameData data)
    {
        string fullPath = Path.Combine(dataDirPath, DataFileName);
        try
        {
           //Create the  Directory the file will be written to if it doesnt already exist
           Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

           //serialize the C# game data object into JSON 
           string DataToStore = JsonUtility.ToJson(data, true);

           //write the serialized data to the File
           using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(DataToStore);
                }
            }
        }
        catch(Exception e)
        {
            Debug.Log("Error Occured when trying to Save data TO File : " + fullPath + "\n" + e);
        }
    }
}
