using UnityEngine;

public static class FallofGenerator
{

    public static float[,] GenerateFalloffMap(int size){
        float[,] falloffMap = new float[size,size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float yValue = y/(float)size*2-1;
                float xValue = x/(float)size*2-1;

                //variabel för att se om x eller y är närmare kanten av vår kvadrat
                float value = Mathf.Max(Mathf.Abs(yValue), Mathf.Abs(xValue));

                falloffMap[y, x] = Evaluate(value);
            }
        }
        return falloffMap;
    }

    /*
    Följane metod applicerar en ekvation för en kurva som vi sedan kan applicera på våran falloffMap.
    Detta i syfta att få en mer jämn fördelning mellan  mängden "noise" på karten.
    Ekvationen: (x^a)/(x^a+(b-b*x)^a). De satta värdena för a och b kan senare ändras för att få en annan fördelning
    */

    static float Evaluate(float value){
        float a = 3;
        float b = 2.2f;
        
        return Mathf.Pow(value, a)/(Mathf.Pow(value, a) + Mathf.Pow((b-b*value), a));
    }


}
