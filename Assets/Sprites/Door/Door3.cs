using UnityEngine;
using UnityEngine.SceneManagement;

public class Door3 : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
         SceneManager.LoadScene(3);

    }

}
