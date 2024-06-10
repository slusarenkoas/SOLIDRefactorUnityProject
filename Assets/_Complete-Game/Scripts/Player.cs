using UnityEngine;
using System.Collections;
using UnityEngine.UI; //Allows us to use UI.
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Completed
{
    public class Player : MovingObject
    {
        [SerializeField] private float _restartLevelDelay = 1f; //Delay time in seconds to restart level.
        [SerializeField] private int _pointsPerFood = 10; //Number of points to add to player food points when picking up a food object.
        [SerializeField] private int _pointsPerSoda = 20; //Number of points to add to player food points when picking up a soda object.
        [SerializeField] private int _wallDamage = 1; //How much damage a player does to a wall when chopping it.
        [SerializeField] private Text _foodText; //UI Text to display current player food total.
        [SerializeField] private AudioClip _moveSound1; //1 of 2 Audio clips to play when player moves.
        [SerializeField] private AudioClip _moveSound2; //2 of 2 Audio clips to play when player moves.
        [SerializeField] private AudioClip _eatSound1; //1 of 2 Audio clips to play when player collects a food object.
        [SerializeField] private AudioClip _eatSound2; //2 of 2 Audio clips to play when player collects a food object.
        [SerializeField] private AudioClip _drinkSound1; //1 of 2 Audio clips to play when player collects a soda object.
        [SerializeField] private AudioClip _drinkSound2; //2 of 2 Audio clips to play when player collects a soda object.
        [SerializeField] private AudioClip _gameOverSound; //Audio clip to play when player dies.

        private Food _food;

        private IGetInput _input;
        private ICharacterAnimations _characterAnimations;

        protected override void Start()
        {
            _characterAnimations = new PlayerAnimation(GetComponent<Animator>());

            _food = FoodManager.Instance.Get() ?? new Food(GameManager.instance.Config.PlayerFoodPoints);

            _foodText.text = "Food: " + _food.Amount;

            base.Start();
        }
        
        private void Update()
        {
            if (!GameManager.instance._playersTurn) return;

            var input = _input.GetInput();
            var horizontal = input.x;
            var vertical = input.y;

            if (horizontal != 0) vertical = 0;
			
            if (horizontal != 0 || vertical != 0)
                AttemptMove<Wall>(horizontal, vertical);
        }
        
        public void SetInput(IGetInput input)
        {
            _input = input;
        }
        
        public void LoseFood(int loss)
        {
            _characterAnimations.SetPlayerHit();
            
            _food.Remove(loss);

            _foodText.text = "-" + loss + " Food: " + _food;
            CheckIfGameOver();
        }

        private void OnDisable()
        {
            FoodManager.Instance.Save(_food);
        }
        
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            _food.Remove(1);

            _foodText.text = "Food: " + _food.Amount;
            base.AttemptMove<T>(xDir, yDir);

            //Hit allows us to reference the result of the Linecast done in Move.
            RaycastHit2D hit;

            //If Move returns true, meaning Player was able to move into an empty space.
            if (Move(xDir, yDir, out hit))
                //Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
                SoundManager.instance.RandomizeSfx(_moveSound1, _moveSound2);

            //Since the player has moved and lost food points, check if the game has ended.
            CheckIfGameOver();

            //Set the playersTurn boolean of GameManager to false now that players turn is over.
            GameManager.instance._playersTurn = false;
        }


        //OnCantMove overrides the abstract function OnCantMove in MovingObject.
        //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
        protected override void OnCantMove<T>(T component)
        {
            var hitWall = component as Wall;

            hitWall.DamageWall(_wallDamage);

            _characterAnimations.SetAttack();
        }


        //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
        private void OnTriggerEnter2D(Collider2D other)
        {
            //Check if the tag of the trigger collided with is Exit.
            if (other.tag == "Exit")
            {
                //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
                Invoke("Restart", _restartLevelDelay);

                //Disable the player object since level is over.
                enabled = false;
            }

            //Check if the tag of the trigger collided with is Food.
            else if (other.tag == "Food")
            {
                //Add pointsPerFood to the players current food total.
                _food.Remove(_pointsPerFood);

                //Update foodText to represent current total and notify player that they gained points
                _foodText.text = "+" + _pointsPerFood + " Food: " + _food;

                //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
                SoundManager.instance.RandomizeSfx(_eatSound1, _eatSound2);

                //Disable the food object the player collided with.
                other.gameObject.SetActive(false);
            }

            //Check if the tag of the trigger collided with is Soda.
            else if (other.tag == "Soda")
            {
                //Add pointsPerSoda to players food points total
                _food.Add(_pointsPerSoda);

                //Update foodText to represent current total and notify player that they gained points
                _foodText.text = "+" + _pointsPerSoda + " Food: " + _food;

                //Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
                SoundManager.instance.RandomizeSfx(_drinkSound1, _drinkSound2);

                //Disable the soda object the player collided with.
                other.gameObject.SetActive(false);
            }
        }


        //Restart reloads the scene when called.
        private void Restart()
        {
            //Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
            //and not load all the scene object in the current scene.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }
        
        //CheckIfGameOver checks if the player is out of food points and if so, ends the game.
        private void CheckIfGameOver()
        {
            //Check if food point total is less than or equal to zero.
            if (_food.Amount <= 0)
            {
                //Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
                SoundManager.instance.PlaySingle(_gameOverSound);

                //Stop the background music.
                SoundManager.instance._musicSource.Stop();

                //Call the GameOver function of GameManager.
                GameManager.instance.GameOver();
            }
        }
    }
}