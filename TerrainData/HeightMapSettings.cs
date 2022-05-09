using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : Updateable {
	public NoiseSettings noiseSettings;
	public bool applyFalloffAesthetics;
	public float heightMultiplier;
	public AnimationCurve heightCurve;
	public float lowest {
        get{
            return heightMultiplier * heightCurve.Evaluate(0);
        }
    }

    public float highest{
        get{
            return heightMultiplier * heightCurve.Evaluate(1); 
        }
    }

	#if UNITY_EDITOR

	protected override void OnValidate() {
		noiseSettings.ValidateValues();

		base.OnValidate ();
	}

	#endif

}