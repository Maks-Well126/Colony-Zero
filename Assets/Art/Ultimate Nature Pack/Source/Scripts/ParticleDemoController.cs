using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ParticleDemoController : MonoBehaviour {

	public List<GameObject> _ParticleEffects = new List<GameObject>();
	public Text CurrentParticleEffectName;
	int ParticleToEnable = 0;
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.D)){
			if (ParticleToEnable < _ParticleEffects.Count-1){
				ParticleToEnable++;
			}

			_ParticleEffects[ParticleToEnable].SetActive(true);
			CurrentParticleEffectName.text = "Current Particle Effect: " + _ParticleEffects[ParticleToEnable].name;

			if (ParticleToEnable < _ParticleEffects.Count){
				_ParticleEffects[ParticleToEnable-1].SetActive(false);
			}
		}
		if (Input.GetKeyDown(KeyCode.A)){
			if (ParticleToEnable > 0){
				ParticleToEnable--;
			}

			_ParticleEffects[ParticleToEnable].SetActive(true);
			CurrentParticleEffectName.text = "Current Particle Effect: " + _ParticleEffects[ParticleToEnable].name;

			if (ParticleToEnable >= 0){
				_ParticleEffects[ParticleToEnable+1].SetActive(false);
			}
		}
	}
}
