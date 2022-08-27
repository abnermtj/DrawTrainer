using System.Collections;
using SimpleMenuController.Runtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Sample.Menus
{
    public class MainMenu : Menu
    {
        [Header("Buttons")]
        public Button Button1;

        public Button Button2;
        public Button Button3;
        public Button Button4;
        public Button Button5;

        public void Quit()
        {
            Application.Quit();
        }

        public override void Open(Menu previousMenu)
        {
            base.Open(previousMenu);

            ShowButtons(false);
            ShowButtons(true);
        }

        public void startScene(string name)
        {
            SceneManager.LoadScene(name);
        }

        //    StartCoroutine(ShowButtonsDelayed());
        //}

        //private IEnumerator ShowButtonsDelayed()
        //{
        //    yield return new WaitForSeconds(1f);
        //}

        private void ShowButtons(bool show)
        {
            Button1.gameObject.SetActive(show);
            Button2.gameObject.SetActive(show);
            Button3.gameObject.SetActive(show);
            Button4.gameObject.SetActive(show);
            Button5.gameObject.SetActive(show);
        }
    }
}