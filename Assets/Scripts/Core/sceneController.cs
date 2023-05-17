using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hyo.core
{
    public class sceneController : MonoBehaviour
    {
        public void NewScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void MoveGame()
        {
            SceneManager.LoadScene(Constants.NameOfGameScene);
        }

        public void MoveJsonUtilScene()
        {
            SceneManager.LoadScene(Constants.NameOfCellGridDesignSceneName);
        }
    }
}


