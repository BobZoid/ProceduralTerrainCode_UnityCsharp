using UnityEngine;
//using System.Linq;

[CreateAssetMenu()]
public class TextureSettings : Updateable
{

    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;
    //Max fem lager i nuvarande utformning
    public Layer[] layers = new Layer[5];
    public void ApplyToMaterial(Material material) {

        /*
        Texture2DArray textureArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        material.SetTexture("Texture2DArray_24ac79c316ce4f5c9b8193dbf754cffd", textureArray);
        */

        material.SetFloat("Vector1_75997bc98ce048758f68b0607a490a8f", layers[0].startHeight);//startHeightOne
        material.SetFloat("Vector1_928909278d794c75a1d0ca8d9467a88d", layers[1].startHeight);//startHeightTwo
        material.SetFloat("Vector1_9cd537d7d0f04645b80a1e7081efa4c7", layers[2].startHeight);//startHeightThree
        material.SetFloat("Vector1_5b308129331942e0a810452b7370e4bc", layers[3].startHeight);//startHeightFour
        material.SetFloat("Vector1_e9a9e6a9a7424fbdb0ab7977b889a391", layers[4].startHeight);//startHeightFive

        material.SetTexture("Texture2D_0ed15d9c9b9245d5afb2badc4ae6eca3", layers[0].texture);//textureOne
        material.SetTexture("Texture2D_daf98734cc264e20a1058aeab8f62ce8", layers[1].texture);//textureTwo
        material.SetTexture("Texture2D_1f837602e7a947dea90f7b52e10d2bb4", layers[2].texture);//textureThree
        material.SetTexture("Texture2D_16ebbe87323444b79362a62e012da359", layers[3].texture);//textureFour
        material.SetTexture("Texture2D_c4ff88ca2c684028ac075e01f431eb82", layers[4].texture);//textureFive

        material.SetFloat("Vector1_4c2aa072925c4f10850e842892e6627c", layers[0].textureScale);//textureOneSize
        material.SetFloat("Vector1_3f2b2546635b4a95a217696e82e74a1b", layers[1].textureScale);//textureTwoSize
        material.SetFloat("Vector1_13ce65a0a52c4a55b9b2dd2b35ce5c6d", layers[2].textureScale);//textureThreeSize
        material.SetFloat("Vector1_14064e6c95854e7590b9ce7cccae7b12", layers[3].textureScale);//textureFourSize
        material.SetFloat("Vector1_f301a5daa2344d31905467b085b421e9", layers[4].textureScale);//textureFiveSize

		//UpdateMeshHeights (material, savedHighest, savedLowest);
	}

    Texture2DArray GenerateTextureArray(Texture2D[] textures){
        //Ang Texture2Darray: Alla texturer måste ha samma width och height värde. Depth = antalet texturer. Format är bildernas format. 
        //Värt att betänka är att texturerna kan behöva sättas till samma värden även i Unity
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);
        for (int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }
        textureArray.Apply();
        return textureArray;
    }

    [System.Serializable]
    public class Layer{
        public Texture2D texture;
        //public Color wash;
        //[Range(0,1)]
        //public float washIntensity;
        public float startHeight;
        //[Range(0,1)]
        //public float blendStrength;
        public float textureScale;
    }
}
