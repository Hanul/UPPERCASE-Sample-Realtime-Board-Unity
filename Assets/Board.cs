using Uniconn;
using UnityEngine;

public class Board : MonoBehaviour
{
    void Start()
    {
        Connector connector = new Connector("127.0.0.1", false, 8530, 8531);

        connector.Connect(() =>
        {
            Debug.Log("접속에 실패하였습니다.");
        }, () =>
        {
            Debug.Log("접속되었습니다.");
        }, () =>
        {
            Debug.Log("연결이 끊어졌습니다.");
        });
    }
    
    public void Write()
    {

    }

    public void Submit()
    {
        
    }
}
