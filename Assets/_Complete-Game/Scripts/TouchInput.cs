using UnityEngine;

namespace Completed
{
    public class TouchInput : IGetInput
    {
        private Vector2 _touchOrigin = -Vector2.one;
        
        public Vector2Int GetInput()
        {
            if (Input.touchCount <= 0) return Vector2Int.zero;
            
            Touch myTouch = Input.touches[0];
            if (myTouch.phase == TouchPhase.Began)
            {
                _touchOrigin = myTouch.position;
            }
				
            else if (myTouch.phase == TouchPhase.Ended && _touchOrigin.x >= 0)
            {
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - _touchOrigin.x;
                float y = touchEnd.y - _touchOrigin.y;
                _touchOrigin.x = -1;
                    
                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    var horizontal = x > 0 ? 1 : -1;
                    return new Vector2Int(horizontal, 0);
                }
                
                var vertical = y > 0 ? 1 : -1;;
                return new Vector2Int(0, vertical);
            }

            return Vector2Int.zero;
        }
    }
}