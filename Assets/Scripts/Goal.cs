using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private bool scored = false;
    public static System.Action<string> Event_GoalScored;
    void OnTriggerEnter(Collider col) {
        //Debug.Log(col.gameObject.name + " entered " + gameObject.name);
        if (col.gameObject.name == "Ball" && !scored) {
            scored = true;
            Score(col.gameObject);
        }
    }
    async void Score(GameObject in_ball) {
        Event_GoalScored?.Invoke(gameObject.name);

        // Wait a second before reseting the ball
        await(Task.Delay(500));
        in_ball.transform.position = new Vector3(0, 2f, 0);
        in_ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        scored = false;
    }
}
