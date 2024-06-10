using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Serialization;

namespace Completed
{
    using System.Collections.Generic; //Allows us to use Lists. 
    using UnityEngine.UI; //Allows us to use UI.
    
    [Serializable]
    public class GameConfig
    {
        [field: SerializeField] public float LevelStartDelay { get; private set; } = 2f;
        [field: SerializeField] public float TurnDelay { get; private set; } = 0.1f;
        [field: SerializeField] public int PlayerFoodPoints { get;private set; } = 100;
    }

    public class GameManager : MonoBehaviour
    {
        [field: SerializeField] public GameConfig Config { get; private set; }

        public static GameManager
            instance = null; //Static instance of GameManager which allows it to be accessed by any other script.

        [FormerlySerializedAs("playersTurn")] [HideInInspector]
        public bool _playersTurn = true; //Boolean to check if it's players turn, hidden in inspector but public.


        private Text _levelText; 
        private GameObject _levelImage;
        private BoardManager _boardScript;
        private int _level = 1; 
        private List<Enemy> _enemies;
        private bool _enemiesMoving; 

        private bool _doingSetup = true; 
        private void Awake()
        {
            //Check if instance already exists
            if (instance == null)

                //if not, set instance to this
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);

            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);

            //Assign enemies to a new List of Enemy objects.
            _enemies = new List<Enemy>();

            //Get a component reference to the attached BoardManager script
            _boardScript = GetComponent<BoardManager>();

            //Call the InitGame function to initialize the first level 
            InitGame();
        }

        //this is called only once, and the paramter tell it to be called only after the scene was loaded
        //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void CallbackInitialization()
        {
            //register the callback to be called everytime the scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //This is called each time a scene is loaded.
        private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance._level++;
            instance.InitGame();
        }


        //Initializes the game for each level.
        private void InitGame()
        {
            //While doingSetup is true the player can't move, prevent player from moving while title card is up.
            _doingSetup = true;

            //Get a reference to our image LevelImage by finding it by name.
            _levelImage = GameObject.Find("LevelImage");

            //Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
            _levelText = GameObject.Find("LevelText").GetComponent<Text>();

            //Set the text of levelText to the string "Day" and append the current level number.
            _levelText.text = "Day " + _level;

            //Set levelImage to active blocking player's view of the game board during setup.
            _levelImage.SetActive(true);

            //Call the HideLevelImage function with a delay in seconds of levelStartDelay.
            Invoke("HideLevelImage", Config.LevelStartDelay);

            //Clear any Enemy objects in our List to prepare for next level.
            _enemies.Clear();

            //Call the SetupScene function of the BoardManager script, pass it current level number.
            _boardScript.SetupScene(_level);
        }


        //Hides black image used between levels
        private void HideLevelImage()
        {
            //Disable the levelImage gameObject.
            _levelImage.SetActive(false);

            //Set doingSetup to false allowing player to move again.
            _doingSetup = false;
        }

        //Update is called every frame.
        private void Update()
        {
            //Check that playersTurn or enemiesMoving or doingSetup are not currently true.
            if (_playersTurn || _enemiesMoving || _doingSetup)

                //If any of these are true, return and do not start MoveEnemies.
                return;

            //Start moving enemies.
            StartCoroutine(MoveEnemies());
        }

        //Call this to add the passed in Enemy to the List of Enemy objects.
        public void AddEnemyToList(Enemy script)
        {
            //Add Enemy to List enemies.
            _enemies.Add(script);
        }


        //GameOver is called when the player reaches 0 food points
        public void GameOver()
        {
            //Set levelText to display number of levels passed and game over message
            _levelText.text = "After " + _level + " days, you starved.";

            //Enable black background image gameObject.
            _levelImage.SetActive(true);

            //Disable this GameManager.
            enabled = false;
        }

        //Coroutine to move enemies in sequence.
        private IEnumerator MoveEnemies()
        {
            //While enemiesMoving is true player is unable to move.
            _enemiesMoving = true;

            //Wait for turnDelay seconds, defaults to .1 (100 ms).
            yield return new WaitForSeconds(Config.TurnDelay);

            //If there are no enemies spawned (IE in first level):
            if (_enemies.Count == 0)
                //Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
                yield return new WaitForSeconds(Config.TurnDelay);

            //Loop through List of Enemy objects.
            for (var i = 0; i < _enemies.Count; i++)
            {
                //Call the MoveEnemy function of Enemy at index i in the enemies List.
                _enemies[i].MoveEnemy();

                //Wait for Enemy's moveTime before moving next Enemy, 
                yield return new WaitForSeconds(_enemies[i]._moveTime);
            }

            //Once Enemies are done moving, set playersTurn to true so player can move.
            _playersTurn = true;

            //Enemies are done moving, set enemiesMoving to false.
            _enemiesMoving = false;
        }
    }
}