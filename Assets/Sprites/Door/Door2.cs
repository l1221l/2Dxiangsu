using UnityEngine;
using UnityEngine.SceneManagement;

public class Door2 : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
         SceneManager.LoadScene(2);

    }

}
