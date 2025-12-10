using UnityEngine;
 // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static class GameContext
    {
        public enum CharacterType { 
        Melee, //‹ß 
        Ranged //‰“
    }

    public static CharacterType SelectedCharacter;

    public static float[] LevelTimes = new float[3];


    public static void ResetData()
    {
        LevelTimes = new float[3]; 
    }
}

