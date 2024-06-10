namespace Completed
{
    public class FoodManager
    {
        private Food _storedFood;
        private static FoodManager _instance;
        
        public static FoodManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FoodManager();
                }

                return _instance;
            }
        }

        public void Save(Food food)
        {
            _storedFood = food;
        }

        public Food Get()
        {
            return _storedFood;
        }
        
        private FoodManager(){}
    }
}