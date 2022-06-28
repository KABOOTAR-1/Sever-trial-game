using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int ,PlayerManager> players = new Dictionary<int ,PlayerManager>();

    public GameObject localPrefab;
    public GameObject PlayerPrefab;
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

    public void SpwanPlayer(int _id, string _username,Vector3 _position,Quaternion _rotation)
    {
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player=Instantiate(localPrefab,position:_position,rotation:_rotation);
        }
        else
        {
            _player=Instantiate(PlayerPrefab,position:_position,rotation:_rotation);
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().USerName = _username;
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }
}
