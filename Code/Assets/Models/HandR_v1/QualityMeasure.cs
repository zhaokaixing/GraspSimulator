using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QualityMeasure
{
    public Vector3? SubjectCenterOfMass { get; set; }

    [System.NonSerialized] public double GraspQuality = 0.0;

    public QualityMeasure(Vector3 subjectCenterOfMass)
    {
        SubjectCenterOfMass = subjectCenterOfMass;
    }


    // calculate the maximum wrench that could be resisted at the point of contact
    // with applied finger force having an magnitude of 1
    // TODO determine frictionCoefficient
    private double CalculateLocalMaximumWrench(ContactPoint contactPoint, double frictionCoefficient = 0.4)
    {
        Vector3 torqueAxis = contactPoint.point - SubjectCenterOfMass.Value;
        Vector3 normalForce = contactPoint.normal.normalized;
        // TODO consider friction force
        Vector3 force = normalForce;
        Vector3 torque = Vector3.Cross(torqueAxis, force);

        // scalar for torque in relation to force
        const double lambda = 1;
        //double quality = Math.Abs(Math.Sqrt(force.sqrMagnitude + lambda * torque.sqrMagnitude));
        // simplified
        double quality = force.sqrMagnitude + lambda * torque.sqrMagnitude;
        return quality;
    }

    public double CalculateGraspQuality(List<ContactPoint> contacts)
    {
        List<double> localQualityScores = new List<double>();

        foreach (var contactPoint in contacts)
        {
            localQualityScores.Add(CalculateLocalMaximumWrench(contactPoint));
        }

        GraspQuality = localQualityScores.Min();
        return GraspQuality;

    }

}
