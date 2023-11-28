using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
	[System.Serializable]
	public enum SpawnState { SPAWNING, WAITING, COUNTING };

	[System.Serializable]
	public class Wave
	{
		public string name; 
		public UnityEvent OnWaveEnd;
		[SerializeField] public EnemyController[] enemies;
		[SerializeField]public int count;
		[SerializeField]public float rate;
		public bool shouldWaitWaveClear = true;
	}

	public Wave[] waves;
	private int nextWave = 0;
	private int currentWave = 0;

	public int NextWave
	{
		get { return nextWave + 1; }
	}

	public GameObject spawnPoints;
	public float timeBetweenWaves = 5f;
	public UnityEvent OnWaveEnd;
	
	private float waveCountdown;
	public float WaveCountdown
	{
		get { return waveCountdown; }
	}

	private float searchCountdown = 1f;

	private SpawnState state = SpawnState.COUNTING;
	public SpawnState State
	{
		get { return state; }
	}

	void Start()
	{
	
		waveCountdown = timeBetweenWaves;
	}

	void Update()
	{

		if (state == SpawnState.WAITING)
		{
			if (!EnemyIsAlive() || !waves[currentWave].shouldWaitWaveClear)
			{
				WaveCompleted(waves[currentWave]);
			}
			else
			{
				return;
			}
		}

		if (waveCountdown <= 0 || nextWave == 0)
		{
			if (state != SpawnState.SPAWNING)
			{
				StartCoroutine(SpawnWave(waves[nextWave]));
			}
		}
		else
		{
			waveCountdown -= Time.deltaTime;
		}
	}

	void WaveCompleted(Wave _wave)
	{
		Debug.Log("Wave Completed!");

		_wave.OnWaveEnd.Invoke();

		state = SpawnState.COUNTING;
		waveCountdown = timeBetweenWaves;

		if (nextWave + 1 > waves.Length - 1)
		{

			OnWaveEnd.Invoke();
			Debug.Log("ALL WAVES COMPLETE! Looping...");
			this.enabled = false;
		}
		else
		{
			nextWave++;
			currentWave++;
		}
	}

	bool EnemyIsAlive()
	{
		searchCountdown -= Time.deltaTime;
		if (searchCountdown <= 0f)
		{
			searchCountdown = 1f;
			if (GameObject.FindGameObjectWithTag("Enemy") == false)
			{
				return false;
			}
		}
		return true;
		
	}

	IEnumerator SpawnWave(Wave _wave)
	{
		Debug.Log("Spawning Wave: " + _wave.name);

		state = SpawnState.SPAWNING;

		for (int i = 0; i < waves[currentWave].enemies.Length; i++)
		{
			Instantiate(waves[currentWave].enemies[i], spawnPoints.transform);
			yield return new WaitForSeconds(1f / _wave.rate);
		}

		state = SpawnState.WAITING;

		yield break;
	}
}