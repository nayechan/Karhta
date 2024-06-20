using System;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Transform pageHolder;
        [SerializeField] private string defaultPageName;
        [SerializeField] private Player.Player player;
        
        private Dictionary<string, MenuPage> menuPages;
        private Stack<MenuPage> pageStack;
        private MenuPage openedPage = null;
        
        public void Awake()
        {
            menuPages = new Dictionary<string, MenuPage>();
            pageStack = new Stack<MenuPage>();
            
            foreach (Transform pageTransform in pageHolder)
            {
                var menuPage = pageTransform.GetComponent<MenuPage>();

                if (menuPage != null)
                {
                    menuPages.Add(pageTransform.name, menuPage);
                }
            }    
            
            OpenPage(defaultPageName);
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.None;
            OpenPage(defaultPageName);
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.Confined;
            openedPage.gameObject.SetActive(false);
        }

        public void OpenPage(string pageName)
        {
            if (menuPages.TryGetValue(pageName, out var menuPage))
            {
                if(openedPage != null)
                    openedPage.gameObject.SetActive(false);
                
                openedPage = menuPage;
                
                if(pageStack.Count == 0 || pageStack.Peek() != openedPage)
                    pageStack.Push(openedPage);
                
                openedPage.gameObject.SetActive(true);
            }
        }

        public void GoBack()
        {
            pageStack.Pop();

            if (pageStack.TryPeek(out var topPage))
            {
                OpenPage(topPage.name);
            }

            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
