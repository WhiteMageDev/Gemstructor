using System;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private IDataService DataService = new JsonDataService();

    void CheckLvlDataFile()
    {
        if (!DataService.IsFileExists(Utils.pathLvlData))
        {
            LvlData lvlData = new();
            lvlData.Lvls = new();
            DataService.SaveData(Utils.pathLvlData, lvlData);
        }
        else
        {
            try
            {
                LvlData lvlData = DataService.LoadData<LvlData>(Utils.pathLvlData);
                if (!(lvlData.Lvls != null && lvlData.power >= 0))
                    throw new CustomException("LvlData data incorrect");
            }
            catch (Exception)
            {
                LvlData lvlData = new();
                lvlData.Lvls = new();
                DataService.SaveData(Utils.pathLvlData, lvlData);
            }
        }
    }
    void CheckAchievementsFile()
    {
        if (!DataService.IsFileExists(Utils.pathAchievements))
        {
            Achievements lvlData = new();
            DataService.SaveData(Utils.pathAchievements, lvlData);
        }
        else
        {
            try
            {
                Achievements lvlData = DataService.LoadData<Achievements>(Utils.pathAchievements);
                if (lvlData.CompletedLevels == null || lvlData.DestroyedGems == null || lvlData.DestroyedGlass == null || lvlData.CreatedRegularBombs == null || lvlData.CreatedTshapeBombs == null)
                    throw new CustomException("Achievements data incorrect");
            }
            catch (Exception)
            {
                Achievements lvlData = new();
                DataService.SaveData(Utils.pathAchievements, lvlData);
            }
        }
    }

    private void Start()
    {

        //  Creating save files.
        //  File paths are contained in the Utils class.

        //  If the file does not exist, create a new file.

        //  Else

        //  Checking the data, if the data is correct we do nothing.
        //  If the data is incorrect -  throw Custom Exception.
        //  Create a new file in the catch block.

        CheckLvlDataFile();
        CheckAchievementsFile();
    }
}

[Serializable]
public class CustomException : Exception
{
    public CustomException(string message) : base(message) { }
}
