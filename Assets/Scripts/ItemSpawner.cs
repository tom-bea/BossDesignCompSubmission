using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemSpawner : MonoBehaviour
{
   public static int curNumOfItems;

   #region Editor Variables
   [SerializeField]
   [Tooltip("Spawn positions for items.")]
   private GameObject[] m_SpawnPositions;

   [SerializeField]
   [Tooltip("Maximum number of items on screen at any time.")]
   private int m_MaxItems = 2;

   [SerializeField]
   [Tooltip("The items to spawn")]
   private GameObject[] m_Items;
   #endregion

   #region Main Updates
   private void Awake()
   {
      foreach (var item in m_Items)
         StartCoroutine(Spawn(item));
   }
   #endregion

   private IEnumerator Spawn(GameObject item)
   {
      while (true)
      {
         yield return null;
         if (!GameManager.GameRunning)
            continue;
         if (curNumOfItems == m_MaxItems)
            continue;

         yield return new WaitForSeconds(1.5f);

         curNumOfItems++;
         Instantiate(item, m_SpawnPositions[Random.Range(0, m_SpawnPositions.Length)].transform.position, Quaternion.identity);
      }
   }
}
