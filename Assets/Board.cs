using System;
using System.Collections.Generic;
using Uniconn;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    private const string HOST = "sample-realtime-board.uppercase.io";
    private const int WEB_SERVER_PORT = 8530;
    private const int SOCKET_SERVER_PORT = 8529;

    [Serializable]
    public class ArticleData
    {
        public string id;
        public string title;
        public string content;

        public DateTime createTime;
        public DateTime lastUpdateTime;
    }

    [Serializable]
    public class ArticleDataValidErrors
    {
        public ValidError title;
        public ValidError content;
    }

    [Serializable]
    public class ArticleResult
    {
        public string errorMsg;
        public ArticleDataValidErrors validErrors;
        public bool isNotAuthed;
        public ArticleData savedData;
    }

    [Serializable]
    public class ArticleCountResult
    {
        public string errorMsg;
        public bool isNotAuthed;
        public int count;
    }

    [Serializable]
    public class ArticleListResult
    {
        public string errorMsg;
        public bool isNotAuthed;
        public List<ArticleData> savedDataSet;
    }

    [Serializable]
    public class FindArticleSort
    {
        public int createTime = -1;
    }

    [Serializable]
    public class FindArticleParams
    {
        public FindArticleSort sort;
        public int start;
        public int count;
    }

    private Connector connector;

    private Room articleRoom;

    private int articleCountPerPage = 10;
    private int totalPage;

    private List<ArticleData> articleDataSet;
    private List<Room> articleWatchingRooms = new List<Room>();

    private GameObject form;
    private GameObject alert;
    private GameObject article;

    private string alertMsg;
    private bool isToCancelForm;

    private ArticleData nowArticleData;

    public void FindArticles(int page)
    {
        FindArticleParams findParams = new FindArticleParams();

        FindArticleSort findSort = new FindArticleSort();
        findSort.createTime = -1;
        findParams.sort = findSort;

        findParams.start = (page - 1) * articleCountPerPage;
        findParams.count = articleCountPerPage;

        articleRoom.Send<ArticleListResult>("find", findParams, (articleListResult) =>
        {
            // 기존 와칭 룸 모두 제거
            foreach (Room articleWatchingRoom in articleWatchingRooms)
            {
                articleWatchingRoom.Exit();
            }
            articleWatchingRooms.Clear();

            articleDataSet = articleListResult.savedDataSet;

            // 글 별 와칭 룸 생성
            foreach (ArticleData articleData in articleDataSet)
            {
                Room articleWatchingRoom = new Room(connector, "RealtimeBoard", "Article/" + articleData.id);
                articleWatchingRoom.On<ArticleData>("update", (ArticleData savedData) =>
                {
                    articleData.title = savedData.title;
                    articleData.content = savedData.content;
                });
                articleWatchingRoom.On("remove", (object notUsing) =>
                {
                    articleDataSet.Remove(articleData);
                    articleWatchingRooms.Remove(articleWatchingRoom);

                    // 페이지 새로고침
                    FindArticles(page);
                });
                articleWatchingRooms.Add(articleWatchingRoom);
            }
        });
    }

    public void ClearAlertMsg()
    {
        alertMsg = null;
    }

    public void Write()
    {
        form.transform.Find("Title").GetComponent<InputField>().text = "";
        form.transform.Find("Content").GetComponent<InputField>().text = "";

        form.SetActive(true);
    }

    public void Read(int i)
    {
        if (i < articleDataSet.Count)
        {
            nowArticleData = articleDataSet[i];

            article.transform.Find("Title").GetComponent<Text>().text = nowArticleData.title;
            article.transform.Find("Content").GetComponent<Text>().text = nowArticleData.content;

            article.SetActive(true);
        }
    }

    public void CloseArticle()
    {
        CancelRemoveConfirm();

        nowArticleData = null;

        article.SetActive(false);
    }

    public void Modify()
    {
        form.transform.Find("Title").GetComponent<InputField>().text = nowArticleData.title;
        form.transform.Find("Content").GetComponent<InputField>().text = nowArticleData.content;

        form.SetActive(true);
    }

    public void CancelForm()
    {
        form.SetActive(false);
    }

    public void RemoveConfirm()
    {
        article.transform.Find("RemoveButton/RemoveConfirm").gameObject.SetActive(true);
    }

    public void Remove()
    {
        articleRoom.Send("remove", nowArticleData.id);

        CloseArticle();
    }

    public void CancelRemoveConfirm()
    {
        article.transform.Find("RemoveButton/RemoveConfirm").gameObject.SetActive(false);
    }

    public void Submit()
    {
        ArticleData articleData = new ArticleData();
        if (nowArticleData != null)
        {
            articleData.id = nowArticleData.id;
        }
        articleData.title = form.transform.Find("Title").GetComponent<InputField>().text;
        articleData.content = form.transform.Find("Content").GetComponent<InputField>().text;

        articleRoom.Send<ArticleResult>(nowArticleData == null ? "create" : "update", articleData, (ArticleResult result) =>
        {
            ValidError titleError = result.validErrors.title;
            ValidError contentError = result.validErrors.content;

            if (result.errorMsg != null)
            {
                alertMsg = result.errorMsg;
            }
            else if (titleError != null && titleError.type != null)
            {
                if (titleError.type.Equals("notEmpty") == true)
                {
                    alertMsg = "제목을 입력해주세요.";
                }
                else if (titleError.type.Equals("size") == true)
                {
                    alertMsg = "제목은 " + titleError.validParams.max + "글자 이하로 입력해주시기 바랍니다.";
                }
                else
                {
                    alertMsg = "제목이 유효하지 않습니다.";
                }
            }
            else if (contentError != null && contentError.type != null)
            {
                if (contentError.type.Equals("notEmpty") == true)
                {
                    alertMsg = "내용을 입력해주세요.";
                }
                else if (contentError.type.Equals("size") == true)
                {
                    alertMsg = "내용은 " + contentError.validParams.max + "글자 이하로 입력해주시기 바랍니다.";
                }
                else
                {
                    alertMsg = "내용이 유효하지 않습니다.";
                }
            }
            else if (result.isNotAuthed == true)
            {
                alertMsg = "인증되지 않았습니다.";
            }
            else
            {
                isToCancelForm = true;
            }
        });
    }

    public void ViewSourceCode()
    {
        Application.OpenURL("https://github.com/Hanul/UPPERCASE-Sample-Realtime-Board-Unity");
    }

    void Start()
    {
        connector = new Connector();
        connector.RegisterHandlers(() =>
        {
            alertMsg = "서버 접속에 실패하였습니다.";
        }, () =>
        {
            Debug.Log("서버에 접속되었습니다.");
        }, () =>
        {
            alertMsg = "서버와의 연결이 끊어졌습니다.";
        });
        StartCoroutine(connector.Connect(HOST, false, WEB_SERVER_PORT, SOCKET_SERVER_PORT));

        articleRoom = new Room(connector, "RealtimeBoard", "Article");

        articleRoom.Send<ArticleCountResult>("count", new FindArticleParams(), (articleCountResult) =>
        {
            totalPage = (int) Math.Ceiling((double) articleCountResult.count / articleCountPerPage);
        });

        // 새 글이 작성된 경우
        Room onNewArticleRoom = new Room(connector, "RealtimeBoard", "Article/create");
        onNewArticleRoom.On<ArticleData>("create", (ArticleData articleData) =>
        {
            articleDataSet.Insert(0, articleData);
        });

        // 첫 페이지 로딩
        FindArticles(1);

        // UI 요소들 가져오기
        form = transform.Find("Form").gameObject;
        form.SetActive(false);

        alert = transform.Find("Alert").gameObject;
        alert.SetActive(false);

        article = transform.Find("Article").gameObject;
        article.SetActive(false);
    }

    void Update()
    {
        if (articleDataSet != null)
        {
            for (int i = 0; i < 10; i += 1)
            {
                Transform item = transform.Find("List/Item-" + i);
                Text text = item.Find("Text").GetComponent<Text>();

                if (i < articleDataSet.Count)
                {
                    text.text = articleDataSet[i].title;
                }
                else
                {
                    text.text = "";
                }
            }
        }

        for (int page = 1; page <= totalPage; page += 1)
        {
            Transform pageButton = transform.Find("List/Page-" + page);
            if (pageButton != null)
            {
                pageButton.Find("Text").GetComponent<Text>().text = page.ToString();
            }
            else
            {
                pageButton.Find("Text").GetComponent<Text>().text = "";
            }
        }

        if (alertMsg != null)
        {
            alert.SetActive(true);
            alert.transform.Find("Text").GetComponent<Text>().text = alertMsg;
        }
        else
        {
            alert.SetActive(false);
        }

        if (isToCancelForm == true)
        {
            CloseArticle();

            CancelForm();
            isToCancelForm = false;
        }
    }
}
