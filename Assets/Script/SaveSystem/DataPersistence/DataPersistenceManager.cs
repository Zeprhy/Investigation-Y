using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    private GameData gamedata;
    private List<IDataPersistence> dataPersistencesObjects;
    private FileDataHandler dataHandler;
    
    public void Initialize()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.dataPersistencesObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void NewGame()
    {
        this.gamedata = new GameData();
    }

    public void LoadGame()
    {
        // Load data yang di save dari File menggunakan data Handler
        this.gamedata = dataHandler.Load();
        //Mmbuat Gamedata baru jika tidak membpunyai game data
        if (this.gamedata == null)
        {
            Debug.Log("No data was found, Initializing data to defaults");
            NewGame();
        }

        // Load data yang dibutuhkan semua script
        foreach (IDataPersistence dataPersistenceObj in dataPersistencesObjects)
        {
            dataPersistenceObj.LoadData(gamedata);
        } 
    }

    public void SaveGame()
    {
        // Meneruskan data ke script lain agar mereka  bisa di update 
        foreach (IDataPersistence dataPersistenceObj in dataPersistencesObjects)
        {
            dataPersistenceObj.SaveData(ref gamedata);
        } 

        // Save data itu ke File menggunakan Data Handler
        dataHandler.Save(gamedata);
    }

    public void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
  