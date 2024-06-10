using UnityEngine;
using UnityEngine.Serialization;

namespace Completed
{
    public class Enemy : MovingObject
    {
        [FormerlySerializedAs("playerDamage")] public int _playerDamage; 
        [FormerlySerializedAs("attackSound1")] public AudioClip _attackSound1;
        [FormerlySerializedAs("attackSound2")] public AudioClip _attackSound2; 

        private Transform _target;
        private bool _skipMove; 
        private ICharacterAttackAnimation _characterAnimationses;

        protected override void Start()
        {
            GameManager.instance.AddEnemyToList(this);
            _characterAnimationses = new EnemyAnimations(GetComponent<Animator>());
            
            _target = GameObject.FindGameObjectWithTag("Player").transform;

            base.Start();
        }


        //Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
        //See comments in MovingObject for more on how base AttemptMove function works.
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            //Check if skipMove is true, if so set it to false and skip this turn.
            if (_skipMove)
            {
                _skipMove = false;
                return;
            }

            //Call the AttemptMove function from MovingObject.
            base.AttemptMove<T>(xDir, yDir);

            //Now that Enemy has moved, set skipMove to true to skip next move.
            _skipMove = true;
        }


        //MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
        public void MoveEnemy()
        {
            //Declare variables for X and Y axis move directions, these range from -1 to 1.
            //These values allow us to choose between the cardinal directions: up, down, left and right.
            var xDir = 0;
            var yDir = 0;

            //If the difference in positions is approximately zero (Epsilon) do the following:
            if (Mathf.Abs(_target.position.x - transform.position.x) < float.Epsilon)

                //If the y coordinate of the target's (player) position is greater than the y coordinate of this enemy's position set y direction 1 (to move up). If not, set it to -1 (to move down).
                yDir = _target.position.y > transform.position.y ? 1 : -1;

            //If the difference in positions is not approximately zero (Epsilon) do the following:
            else
                //Check if target x position is greater than enemy's x position, if so set x direction to 1 (move right), if not set to -1 (move left).
                xDir = _target.position.x > transform.position.x ? 1 : -1;

            //Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
            AttemptMove<Player>(xDir, yDir);
        }

        protected override void OnCantMove<T>(T component)
        {
            var hitPlayer = component as Player;

            hitPlayer.LoseFood(_playerDamage);
            
            _characterAnimationses.SetAttack();
            SoundManager.instance.RandomizeSfx(_attackSound1, _attackSound2);
        }
    }
}