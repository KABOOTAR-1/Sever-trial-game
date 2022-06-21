using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject StartMenu;
    public TMP_InputField userNameField;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exsists, Destroying Current Object");
            Destroy(gameObject);
        }

    }

    public void ConnectToServer()
    {
        StartMenu.SetActive(false);
        userNameField.interactable = false;
        Client.instance.ConnectToSever();
    }


}
