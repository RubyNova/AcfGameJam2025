using UnityEngine;

namespace Controllers
{
    public class FamiliarYeeter : MonoBehaviour
    {
        [SerializeField] public Transform PositionToYeetTo;

        private int FamiliarHashCode = 0;

        void Start()
        {
            var lf = FindAnyObjectByType<FamiliarController>();
            if(lf is not null)
            {
                FamiliarHashCode = lf.gameObject.GetHashCode();
            }
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if(FamiliarHashCode != 0 && collision.gameObject.GetHashCode() == FamiliarHashCode)
            {
                collision.gameObject.transform.position = PositionToYeetTo.position;
            }
        }
        
    }
}
