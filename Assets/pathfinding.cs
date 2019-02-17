using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class pathfinding : MonoBehaviour
{
    public SteamVR_TrackedController controller_left;
    public SteamVR_TrackedController controller_right;
    public Transform[] targets;

   //private variables
    private NavMeshAgent agent;
    NavMeshPath path;
    LineRenderer line;
    int room_number;

   

    void Start()
    {
        controller_left.PadClicked += leftpadpressed;
        controller_right.PadClicked += rightpadpressed;
        agent = GetComponent<NavMeshAgent>();
        line = GetComponent<LineRenderer>();
        path = new NavMeshPath();
        
        room_number = 0;
    }

    //leftpad button press actions
    void leftpadpressed(object sender, ClickedEventArgs e)
    {      

        if (room_number == targets.Length)
            room_number = 0;

        room_number++;
        if (room_number == targets.Length)
            agent.CalculatePath(targets[targets.Length - 1].position, path);
        else
            agent.CalculatePath(targets[room_number - 1].position, path);

    }


    //rightpad button press actions
    void rightpadpressed(object sender, ClickedEventArgs e)
    {

        if (room_number == -1)
            room_number = targets.Length;
        room_number--;
        if (room_number == -1)
            agent.CalculatePath(targets[targets.Length - 1].position, path);
        else
            agent.CalculatePath(targets[room_number].position, path);
        
    }


    private void Update()
    {
        
        line.positionCount = path.corners.Length;
        line.SetPositions(path.corners);

    }
    //backup to testing//
    //leftpad
    //if (room_number == targets.Length)
    //    room_number = 0;

    //room_number++;
    //if (room_number == targets.Length)
    //    agent.CalculatePath(targets[targets.Length - 1].position, path);
    //else
    //    agent.CalculatePath(targets[room_number - 1].position, path);
    //rightpad
    //if (room_number == -1)
    //        room_number = targets.Length;
    //    room_number--;
    //    if (room_number == -1)
    //        agent.CalculatePath(targets[targets.Length - 1].position, path);
    //    else
    //        agent.CalculatePath(targets[room_number].position, path);
}