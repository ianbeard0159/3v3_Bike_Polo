using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.WebRTC;

using UnityEngine.Networking;

public class networkcontroller : MonoBehaviour
{

    [SerializeField] private string server = "http://rtc.zyugyzarc.repl.co/";
    [SerializeField] private bool host = false;

    [SerializeField] public string gameid;

    private bool connected = false;
    private RTCDataChannel channel;
    private RTCPeerConnection connection;
    private string uuid;


    // Warning : this commented function is cursed and will haunt your frametime
    // async in C# sure is really dumb

    //void wait(IEnumerator coro){
    //    Debug.Log("coro-wait : start");
    //    int i = 100;
    //    while(coro.MoveNext() && (i > 0)){Debug.Log("pong");i--;}
    //    Debug.Log("coro-wait : end");
    //}

    IEnumerator sendSignal(string data){
        // send a POST request to the signaling server

        Debug.Log("bepis");

        var uwr = new UnityWebRequest(server, "POST");
        byte[] bytesToSend = new System.Text.UTF8Encoding().GetBytes(data);
        uwr.uploadHandler = (UploadHandler) new UploadHandlerRaw(bytesToSend);

        uwr.SetRequestHeader("Content-Type", "text/plain");
        uwr.SetRequestHeader("id", uuid);

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log($"Error While Sending: {uwr.error}");
        }

    }

    IEnumerator receiveSignal(){

        while(true){
            UnityWebRequest uwr = UnityWebRequest.Get(server + gameid);
            uwr.SetRequestHeader("id", uuid);

            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log($"Error While Sending: {uwr.error}");
                break;
            }

            if(uwr.responseCode == 200){
                yield return uwr.downloadHandler.text;
                break;
            }
            else{
                Debug.Log("ping");
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    string serializeSDP(RTCSessionDescription sdp){

        return (sdp.type == RTCSdpType.Offer ? "off" : "ans") + $":{sdp.sdp}";

    }

    RTCSessionDescription deserializeSDP(string o){

        Debug.Log(o);

        RTCSessionDescription sdp;

        RTCSdpType t;
        string type = o.Substring(0, 3);
        string sd = o.Substring(4);

        if(type == "off"){t = RTCSdpType.Offer;}
        else if(type == "ans"){t = RTCSdpType.Answer;}
        else{throw new Exception("Bruh Moment");}

        sdp.type = t;
        sdp.sdp = sd;

        return sdp;

    }

    IEnumerator create(){

        // init stuff
        connection = new RTCPeerConnection();
        channel = connection.CreateDataChannel("testchannel");

        // set hooks
        channel.OnOpen = onOpen;
        channel.OnMessage = onMessage;

        connection.OnIceConnectionChange = state => {
            Debug.Log(state);
        };


        // create offer
        var d = connection.CreateOffer();
        yield return d;

        var desc = d.Desc;
        connection.SetLocalDescription(ref desc);

        // do ICE stuff

        connection.OnIceCandidate = ice => {
            bool res = connection.AddIceCandidate(ice);
            Debug.Log($"got ICE {ice.Candidate} ({res})");
        };

        while(connection.GatheringState != RTCIceGatheringState.Complete){
            Debug.Log($"waiting...");
            yield return null;
        }

        Debug.Log("got all candidates");

        // re-create offer (with ice)

        d = connection.CreateOffer();
        yield return d;

        desc = d.Desc;
        connection.SetLocalDescription(ref desc);

        // send offer
        string sdp = serializeSDP(desc);
        Debug.Log(sdp);

        yield return sendSignal(sdp);
        Debug.Log("sent sdp");

        // receive answer
        var tmp = receiveSignal();
        yield return tmp;
        sdp = (string) tmp.Current;
        Debug.Log(sdp);

        desc = deserializeSDP(sdp);

        // get answer
        connection.SetRemoteDescription(ref desc);

        // connection should be created by now (hopefully)
    }

    IEnumerator connect(){

        // init stuff
        connection = new RTCPeerConnection();
        connection.OnDataChannel = c => {
            channel = c;
            channel.OnMessage = onMessage;
        };

        // do ICE stuff

        connection.OnIceCandidate = ice => {
            bool res = connection.AddIceCandidate(ice);
            Debug.Log($"got ICE {ice.Candidate} ({res})");
        };

        // receive offer

        var sdp = receiveSignal();
        yield return sdp;

        Debug.Log(sdp.Current);

        var desc = deserializeSDP((string)sdp.Current);

        // set remote desc
        connection.SetRemoteDescription(ref desc);

        // create answer
        var d = connection.CreateAnswer();
        yield return d;

        // do ICE stuff

        connection.OnIceCandidate = ice => {
            bool res = connection.AddIceCandidate(ice);
            Debug.Log($"got ICE {ice.Candidate} ({res})");
        };

        while(connection.GatheringState != RTCIceGatheringState.Complete){
            Debug.Log($"waiting...");
            yield return null;
        }

        // recreate answer for sdp with ices
        d = connection.CreateAnswer();
        yield return d;

        desc = d.Desc;
        connection.SetLocalDescription(ref desc);

        // send answer
        yield return sendSignal( serializeSDP(desc) );

        // i guess thats it... ¯\_(ツ)_/¯

    }

    void onOpen(){
        Debug.Log("connected");
        connected = true;
    }



    // Start is called before the first frame update
    IEnumerator Start()
    {
        // Connects to a client
        // probably needs some sort of UI thing - to be implemented later

        uuid = System.Guid.NewGuid().ToString();

        if(host){

            gameid = System.Guid.NewGuid().ToString();

            yield return create();

            Debug.Log($"Started at {server+gameid}");

        }else{

            // someone good at unity please do something
            // somehow get the user's input into `gameid`, temporarily uses the [SerializeField]
            yield return connect();

            Debug.Log($"connected at {server+gameid}");

        }
        
    }

    void onMessage(byte[] bytes){
        string message = System.Text.Encoding.UTF8.GetString(bytes);
        Debug.Log($">> {message}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
