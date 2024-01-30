using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contact : MonoBehaviour
{
    // prefab for visualising contact points, drag into unity inspector
    public GameObject contactPointPrefab;


    private List<ContactPoint> _contacts = new List<ContactPoint>();
    private List<Object> _visualisedContactPoints = new List<Object>();

    private bool _is_newCollision = false;


    private void OnCollisionStay(Collision collision)
    {

        _is_newCollision = true;
        _contacts.Clear();
        ClearPreviousContactPointsOnScreen();

        for(int i=0; i<collision.contactCount; i++)
        {
            _contacts.Add(collision.GetContact(i));
        }

        VisualiseContactPoints();
    }

    private void OnCollisionExit(Collision collision)
    {
        print("exit");
        _contacts.Clear();
    }

    private void ClearPreviousContactPointsOnScreen()
    {
        foreach (var contactPoint in _visualisedContactPoints)
        {
            Destroy(contactPoint);
        }

        _visualisedContactPoints.Clear();
    }

    private void VisualiseContactPoints()
    {
        foreach (var contact in _contacts)
        {
            Vector3 position = contact.point;
            _visualisedContactPoints.Add(Instantiate(contactPointPrefab, position, Quaternion.identity));
        }
    }

    public void ClearContacts()
    {
        _contacts.Clear();
        ClearPreviousContactPointsOnScreen();
    }

    void OnGUI()
    {
        int Start_x = Screen.width - 200, Start_y = 50, Interval_y = 50;
        int x = Start_x, y = Start_y;
        if (_is_newCollision)
        {
            y = Start_y;
            _is_newCollision = false;
        }

        GUI.Label(new Rect(x, y, 100, 50), string.Format(
            "contact point count:{0}\n", _contacts.Count));
        y += Interval_y;

        for (int i = 0; i < _contacts.Count; i++)
        {
            ContactPoint contactPoint = _contacts[i];
            GUI.Label(new Rect(x, y, 100, 50), string.Format(
                "point:{0}, distance:{1}\n",contactPoint.point, contactPoint.separation));
            y += Interval_y;
        }
    }
}
