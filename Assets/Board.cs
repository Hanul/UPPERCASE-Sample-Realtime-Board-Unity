using System;
using Uniconn;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    [Serializable]
    public class ArticleResult
    {
        public string errorMsg;
        public ArticleDataValidErrors validErrors;
        public bool isNotAuthed;
        public ArticleData savedData;
    }

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

        Room articleRoom = new Room(connector, "RealtimeBoard", "Article");

        ArticleData articleData = new ArticleData();
        articleData.title = "Test Title";
        articleData.content = "Test Content";

        articleRoom.Send<ArticleResult>("create", articleData, (ArticleResult result) =>
        {
            ValidError titleError = result.validErrors.title;
            ValidError contentError = result.validErrors.content;

            if (result.errorMsg != null)
            {
                Debug.LogError(result.errorMsg);
            }
            else if (titleError != null && titleError.type != null)
            {
                Debug.LogError("제목이 유효하지 않습니다.");
            }
            else if (contentError != null && contentError.type != null)
            {
                Debug.LogError("내용이 유효하지 않습니다.");
            }
            else if (result.isNotAuthed == true)
            {
                Debug.LogError("인증되지 않았습니다.");
            }
            else
            {
                Debug.Log(result.savedData.title);
            }
        });

        for (int i = 0; i < 10; i += 1)
        {
            Transform item = transform.Find("List/Item-" + i);
            Text text = item.Find("Text").GetComponent<Text>();
            text.text = "TEST";
        }
    }
    
    public void Write()
    {

    }

    public void Submit()
    {
        
    }
}
