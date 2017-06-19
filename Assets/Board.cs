using Uniconn;
using UnityEngine;

public class Board : MonoBehaviour
{
    void Start()
    {
        Connector connector = new Connector();

        connector.RegisterHandlers(() =>
        {
            Debug.Log("접속에 실패하였습니다.");
        }, () =>
        {
            Debug.Log("접속되었습니다.");
        }, () =>
        {
            Debug.Log("연결이 끊어졌습니다.");
        });

        StartCoroutine(connector.Connect("127.0.0.1", false, 8530, 8531));
    }
    
    public void Write()
    {

    }

    public void Submit()
    {
        
    }
}
