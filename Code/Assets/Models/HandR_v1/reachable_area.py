from scipy.spatial import ConvexHull
import numpy as np
import matplotlib.pyplot as plt
from math import sqrt, pi
import sys
from matplotlib.ticker import NullFormatter
import datetime
import os
from pathlib import Path
from mpl_toolkits import mplot3d
from scipy.spatial import Delaunay


# def computeReachArea(vertices, nbgraph, labelAbs, labelOrd):
def computeReachArea(vertices, touchT, touchI, touchM, touchR, nbgraph, labelAbs, labelOrd):
    """ Display the non-rect object and the surface area touched by each finger """
    try:
        print(vertices)
        print(touchT)
        print(touchI)
        print(touchM)
        print(touchR)
        
        hull_object = ConvexHull(vertices)
        hull_t = ConvexHull(touchT)
        hull_i = ConvexHull(touchI)
        hull_m = ConvexHull(touchM)
        hull_r = ConvexHull(touchR)

        
        plt.subplot(nbgraph)
        
        plt.axis('equal')

        # Get the surface area of object and touched area by each finger
        # hull_object.volume
        # hull_object.area
        print(str(labelAbs) + str(labelOrd))
        #objectarea = hull_object.volume * cm
        objectarea = hull_object.area * cm
        print('Object Area: ' + str(round(objectarea, 3)))

        reachableT = hull_t.area * cm
        percT = (reachableT / objectarea) * 100
        print('Thumb Reachable Area: ' + str(round(reachableT, 3)) + ' (' + str(round(percT, 3)) + '%) ')

        reachableI = hull_i.area * cm
        percI = (reachableI / objectarea) * 100
        print('Index Reachable Area: ' + str(round(reachableI, 3)) + ' (' + str(round(percI, 3)) + '%) ')

        reachableM = hull_m.area * cm
        percM = (reachableM / objectarea) * 100
        print('Middle Reachable Area: ' + str(round(reachableM, 3)) + ' (' + str(round(percM, 3)) + '%) ')

        reachableR = hull_r.area * cm
        percR = (reachableR / objectarea) * 100
        print('Ring Reachable Area: ' + str(round(reachableR, 3)) + ' (' + str(round(percR, 3)) + '%) ')

        # Draw the Non Rect Object
        plt.plot(vertices[:, 0], vertices[:, 1], ',', color='#7f7f7f', label='Object')

        # Draw Reachable Area by each finger
        plt.plot(touchT[:, 0], touchT[:, 1], '.', color='#4ac3ef', label='Thumb')
        plt.plot(touchI[:, 0], touchI[:, 1], '.', color='#ffe347', label='Index')
        plt.plot(touchM[:, 0], touchM[:, 1], '.', color='#87ea7e', label='Middle')
        plt.plot(touchR[:, 0], touchR[:, 1], '.', color='#ff9eb6', label='Ring')

        # Plot the line between the hull points of touched area
        for simplex in hull_t.simplices:
            plt.plot(touchT[simplex, 0], touchT[simplex, 1], 'c-')

        for simplex in hull_i.simplices:
            plt.plot(touchI[simplex, 0], touchI[simplex, 1], '-', color='#ffe347')

        for simplex in hull_m.simplices:
            plt.plot(touchM[simplex, 0], touchM[simplex, 1], '-', color='#87ea7e')

        for simplex in hull_r.simplices:
            plt.plot(touchR[simplex, 0], touchR[simplex, 1], '-', color='#ff9eb6')

        # Draw the edge of non rect object
        for simplex in hull_object.simplices:
            plt.plot(vertices[simplex, 0], vertices[simplex, 1], 'k-')

        ########### Centroid Computation only for thumb ###########
        # Get the centroid of thumb reachable area
        distance = []
        centroid = np.mean(touchT[hull_t.vertices, :], axis=0)
        plt.plot(centroid[0], centroid[1], 'o')

        # Get the distance to the centroid of all points
        for simplex in hull_t.simplices:
            distance.append(pow(simplex[0] - centroid[0], 2) + (pow(simplex[1] - centroid[1], 2)))
        distance.sort()

        # float => str
        objecttext = str('Object Surface Area: ' + str(round(objectarea, 3)))
        thumbtext = str('Thumb Reachable Area: ' + str(round(reachableT, 3)) + ' (' + str(round(percT, 3)) + '%)')
        indextext = str('Index Reachable Area: ' + str(round(reachableI, 3)) + ' (' + str(round(percI, 3)) + '%) ')
        middletext = str('Middle Reachable Area: ' + str(round(reachableM, 3)) + ' (' + str(round(percM, 3)) + '%) ')
        ringtext = str('Ring Reachable Area: ' + str(round(reachableR, 3)) + ' (' + str(round(percR, 3)) + '%) ')

        plt.xlabel(labelAbs)
        plt.ylabel(labelOrd)
        return (objecttext, thumbtext, indextext, middletext, ringtext)
    # return(objecttext)

    except Exception as e:
        print(e)
        return ([], 0)


# def ConvexHull3D(vertices):
def ConvexHull3D(vertices, touchT, touchI, touchM, touchR):


    try:
        verticesx = np.array([np.array(x[0]) for x in vertices])
        verticesy = np.array([np.array(x[1]) for x in vertices])
        verticesz = np.array([np.array(x[2]) for x in vertices])

        touchTx = np.array([np.array([x[0]]) for x in touchT])
        touchTy = np.array([np.array([x[1]]) for x in touchT])
        touchTz = np.array([np.array([x[2]]) for x in touchT])

        touchIx = np.array([np.array([x[0]]) for x in touchI])
        touchIy = np.array([np.array([x[1]]) for x in touchI])
        touchIz = np.array([np.array([x[2]]) for x in touchI])

        touchMx = np.array([np.array([x[0]]) for x in touchM])
        touchMy = np.array([np.array([x[1]]) for x in touchM])
        touchMz = np.array([np.array([x[2]]) for x in touchM])

        touchRx = np.array([np.array([x[0]]) for x in touchR])
        touchRy = np.array([np.array([x[1]]) for x in touchR])
        touchRz = np.array([np.array([x[2]]) for x in touchR])

        fig = plt.figure()
        ax = plt.axes(projection='3d')

        # ConvexHull
        hull_object = ConvexHull(vertices)
        hull_t = ConvexHull(touchT)

        # plot the vertices and reachable area 
        ax.plot3D(verticesx.flatten(), verticesz.flatten(), verticesy.flatten(), '.', c='gray')
        ax.plot3D(verticesx, verticesz, verticesy, ',', color='gray')

        ax.plot3D(touchTx.flatten(), touchTz.flatten(), touchTy.flatten(), '.', color='#4ac3ef', label='Thumb')
        ax.plot3D(touchIx.flatten(), touchIz.flatten(), touchIy.flatten(), '.', color='#ffe347', label='Index')
        ax.plot3D(touchMx.flatten(), touchMz.flatten(), touchMy.flatten(), '.', color='#87ea7e', label='Middle')
        ax.plot3D(touchRx.flatten(), touchRz.flatten(), touchRy.flatten(), '.', color='#ff9eb6', label='Ring')

        ax.set_xlabel('X')
        ax.set_ylabel('Z')
        ax.set_zlabel('Y')

        return ([], 0)

    except Exception as e:
        print(e)
        return ([], 0)


########### Getting Non Rect Mesh Vertices ###########
try:
    name = sys.argv[1]
    with open("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Data/ContactPoint.txt", "r") as infile:
        vertices = [line.rstrip().split(" ") for line in infile]
        print("vertices = ", vertices)
except:
    # with open("../Results/_p0_r0_0_0_1.txt", "r")  as infile:
    with open("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Data/ContactPoint.txt", "r") as infile:
        vertices = [line.rstrip().split(" ") for line in infile]
    name = "Results/_p0_r0_0_0_1.txt"

# Points are stored in the "x y z \n" format, convert string to float
for i in range(len(vertices)):
    for j in range(len(vertices[i])):
        vertices[i][j] = float(vertices[i][j])

# Get the xy, xz, yz points
verticesxy = np.array([np.array([x[0]] + [x[1]]) for x in vertices])
verticesxz = np.array([np.array([x[0]] + [x[2]]) for x in vertices])
verticesyz = np.array([np.array([x[1]] + [x[2]]) for x in vertices])

# Get the maximum value of x,y coordinates.
xmax = float(0)
ymax = float(0)

for i in range(len(verticesxy)):
    if i == 0:
        xy_xmax = verticesxy[i][0]
        xy_ymax = verticesxy[i][1]
    elif i > 0:
        if verticesxy[i][0] > xy_xmax:
            xy_xmax = verticesxy[i][0]
        if verticesxy[i][1] > xy_ymax:
            xy_ymax = verticesxy[i][1]

for i in range(len(verticesyz)):
    if i == 0:
        yz_xmax = verticesyz[i][0]
        yz_ymax = verticesyz[i][1]
    elif i > 0:
        if verticesyz[i][0] > yz_xmax:
            yz_xmax = verticesyz[i][0]
        if verticesxy[i][1] > yz_ymax:
            yz_ymax = verticesyz[i][1]

for i in range(len(verticesxz)):
    if i == 0:
        xz_xmax = verticesxz[i][0]
        xz_ymax = verticesxz[i][1]
    elif i > 0:
        if verticesxz[i][0] > xz_xmax:
            xz_xmax = verticesxz[i][0]
        if verticesxy[i][1] > xz_ymax:
            xz_ymax = verticesxz[i][1]

####################   Importing four fingers    #####################
# THUMB
# Open file from Assets Python if run in Unity, else from the predefined file       
try:
    name = sys.argv[1]
    with open("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Data/Reachable_T.txt", "r") as infile:
        touchT = [line.rstrip().split(" ") for line in infile]
        print("touchT = ", touchT)
except:
    # with open("../Results/_p0_r0_0_0_1.txt", "r")  as infile:
    with open("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Data/Reachable_T.txt", "r") as infile:
        touchT = [line.rstrip().split(" ") for line in infile]
        name = "Results/_p0_r0_0_0_1.txt"

# Points are stored in the "x y z \n" format, convert into float
for i in range(len(touchT)):
    for j in range(len(touchT[i])):
        touchT[i][j] = float(touchT[i][j])

# Get the xy, xz, yz points
touchTxy = np.array([np.array([x[0]] + [x[1]]) for x in touchT])
touchTxz = np.array([np.array([x[0]] + [x[2]]) for x in touchT])
touchTyz = np.array([np.array([x[1]] + [x[2]]) for x in touchT])

# to remove points much bigger than surface area
for i in range(len(touchTxy)):
    if touchTxy[i][0] > xy_xmax:
        touchTxy[i][0] = xy_xmax

    if touchTxy[i][1] > xy_ymax:
        touchTxy[i][1] = xy_ymax

for i in range(len(touchTyz)):
    if touchTyz[i][0] > yz_xmax:
        touchTyz[i][0] = yz_xmax - 0.2

    if touchTyz[i][1] > yz_ymax:
        touchTyz[i][1] = yz_ymax - 0.2

for i in range(len(touchTxz)):
    if touchTxz[i][0] > xz_xmax:
        touchTxz[i][0] = xz_xmax - 0.2

    if touchTxz[i][1] > xz_ymax:
        touchTxz[i][1] = xz_ymax - 0.2
######################################################################
# INDEX
# Open file from Assets Python if run in Unity, else from the predefined file
try:
    name = sys.argv[1]
    with open("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Data/Reachable_I.txt", "r") as infile:
        touchI = [line.rstrip().split(" ") for line in infile]
        print("touchI = ", touchI)
except:
    # with open("../Results/_p0_r0_0_0_1.txt", "r")  as infile:
    with open("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Data/Reachable_I.txt", "r") as infile:
        touchI = [line.rstrip().split(" ") for line in infile]
        name = "Results/_p0_r0_0_0_1.txt"

# Points are stored in the "x y z \n" format, convert into float
for i in range(len(touchI)):
    for j in range(len(touchI[i])):
        touchI[i][j] = float(touchI[i][j])

# Get the xy, xz, yz points
touchIxy = np.array([np.array([x[0]] + [x[1]]) for x in touchI])
touchIxz = np.array([np.array([x[0]] + [x[2]]) for x in touchI])
touchIyz = np.array([np.array([x[1]] + [x[2]]) for x in touchI])

# to remove points much bigger than surface area
for i in range(len(touchIxy)):
    if touchIxy[i][0] > xy_xmax:
        touchIxy[i][0] = xy_xmax

    if touchIxy[i][1] > xy_ymax:
        touchIxy[i][1] = xy_ymax

for i in range(len(touchIyz)):
    if touchIyz[i][0] > yz_xmax:
        touchIyz[i][0] = yz_xmax - 0.2

    if touchIyz[i][1] > yz_ymax:
        touchIyz[i][1] = yz_ymax - 0.2

for i in range(len(touchIxz)):
    if touchIxz[i][0] > xz_xmax:
        touchIxz[i][0] = xz_xmax - 0.2

    if touchIxz[i][1] > xz_ymax:
        touchIxz[i][1] = xz_ymax - 0.2

######################################################################
# MIDDLE
# Open file from Assets Python if run in Unity, else from the predefined file
try:
    name = sys.argv[1]
    with open("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Data/Reachable_M.txt", "r") as infile:
        touchM = [line.rstrip().split(" ") for line in infile]
        print("touchM = ", touchM)
except:
    # with open("../Results/_p0_r0_0_0_1.txt", "r")  as infile:
    with open("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Data/Reachable_M.txt", "r") as infile:
        touchM = [line.rstrip().split(" ") for line in infile]
        name = "Results/_p0_r0_0_0_1.txt"

# Points are stored in the "x y z \n" format, convert into float
for i in range(len(touchM)):
    for j in range(len(touchM[i])):
        touchM[i][j] = float(touchM[i][j])

# Get the xy, xz, yz points
touchMxy = np.array([np.array([x[0]] + [x[1]]) for x in touchM])
touchMxz = np.array([np.array([x[0]] + [x[2]]) for x in touchM])
touchMyz = np.array([np.array([x[1]] + [x[2]]) for x in touchM])

# to remove points much bigger than surface area

for i in range(len(touchMxy)):
    if touchMxy[i][0] > xy_xmax:
        touchMxy[i][0] = xy_xmax

    if touchMxy[i][1] > xy_ymax:
        touchMxy[i][1] = xy_ymax

for i in range(len(touchMyz)):
    if touchMyz[i][0] > yz_xmax:
        touchMyz[i][0] = yz_xmax - 0.2

    if touchMyz[i][1] > yz_ymax:
        touchMyz[i][1] = yz_ymax - 0.2

for i in range(len(touchMxz)):
    if touchMxz[i][0] > xz_xmax:
        touchMxz[i][0] = xz_xmax - 0.2

    if touchMxz[i][1] > xz_ymax:
        touchMxz[i][1] = xz_ymax - 0.2

######################################################################
# RING
# Open file from Assets Python if run in Unity, else from the predefined file
try:
    name = sys.argv[1]
    with open("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Data/Reachable_R.txt", "r") as infile:
        touchR = [line.rstrip().split(" ") for line in infile]
        print("touchR = ", touchR)
except:
    # with open("../Results/_p0_r0_0_0_1.txt", "r")  as infile:
    with open("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Data/Reachable_R.txt", "r") as infile:
        touchR = [line.rstrip().split(" ") for line in infile]
        name = "Results/_p0_r0_0_0_1.txt"

# Points are stored in the "x y z \n" format, convert into float
for i in range(len(touchR)):
    for j in range(len(touchR[i])):
        touchR[i][j] = float(touchR[i][j])

# Get the xy, xz, yz points
touchRxy = np.array([np.array([x[0]] + [x[1]]) for x in touchR])
touchRxz = np.array([np.array([x[0]] + [x[2]]) for x in touchR])
touchRyz = np.array([np.array([x[1]] + [x[2]]) for x in touchR])

# to remove points much bigger than surface area

for i in range(len(touchRxy)):
    if touchRxy[i][0] > xy_xmax:
        touchRxy[i][0] = xy_xmax

    if touchRxy[i][1] > xy_ymax:
        touchRxy[i][1] = xy_ymax

for i in range(len(touchRyz)):
    if touchRyz[i][0] > yz_xmax:
        touchRyz[i][0] = yz_xmax - 0.2

    if touchRyz[i][1] > yz_ymax:
        touchRyz[i][1] = yz_ymax - 0.2

for i in range(len(touchRxz)):
    if touchRxz[i][0] > xz_xmax:
        touchRxz[i][0] = xz_xmax - 0.2

    if touchRxz[i][1] > xz_ymax:
        touchRxz[i][1] = xz_ymax - 0.2

#######################################################################
# Set unit for cm.
cm = round((8.255 / 2.122), 3)

# Adjust figure size and margin
plt.figure(figsize=(13, 5.5))
plt.suptitle('Reachable Area', fontsize=14)

# Plot the figure
# XY
# oxy = computeReachArea(verticesxy, 131, 'X', 'Y')
oxy, txy, ixy, mxy, rxy = computeReachArea(verticesxy, touchTxy, touchIxy, touchMxy, touchRxy, 131, 'X', 'Y')
plt.title('Front', fontsize=10)

# XZ
# oxz = computeReachArea(verticesxz, 132, 'X', 'Z')
oxz, txz, ixz, mxz, rxz = computeReachArea(verticesxz, touchTxz, touchIxz, touchMxz, touchRxz, 132, 'X', 'Z')
plt.title('Top', fontsize=10)

# YZ
# oyz = computeReachArea(verticesyz, 133, 'Y', 'Z')
oyz, tyz, iyz, myz, ryz = computeReachArea(verticesyz, touchTyz, touchIyz, touchMyz, touchRyz, 133, 'Y', 'Z')
plt.title('Side', fontsize=10)

plt.subplots_adjust(left=0.08, right=0.95, top=0.82, wspace=0.30, bottom=0.12)

# Save figure
try:
    plt.savefig("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Results/result_image.png")
    print(name)
#   pngName = name.split(".txt")[0].split("/")[3] + ".png"
except Exception as e:
    print(e)
    plt.show()

# plot 3d convex hull
# c = ConvexHull3D(vertices)
c = ConvexHull3D(vertices, touchT, touchI, touchM, touchR)
plt.legend(loc='upper right', ncol=5, fontsize=8, mode="expand")

# Save figure
try:
    plt.savefig("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Results/result_image3d.png")
#   print(name)
#   pngName = name.split(".txt")[0].split("/")[3] + ".png"
except Exception as e:
    print(e)
    plt.show()

# Save computed data in text file.
textfile = Path("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Results/computed_data.txt")
if textfile.is_file():
    os.remove(textfile)

file = open("D:/VS_workspace/GraspSimulator/Assets/Models/HandR_v1/Results/computed_data.txt", 'w')
file.write('1. Front View (XY) \n' + oxy + '\n' + txy + '\n' + ixy + '\n' + mxy + '\n' + rxy + '\n\n')
file.write('2. Top View (XZ) \n' + oxz + '\n' + txz + '\n' + ixz + '\n' + mxz + '\n' + rxz + '\n\n')
file.write('3. Side View (YZ) \n' + oyz + '\n' + tyz + '\n' + iyz + '\n' + myz + '\n' + ryz + '\n\n')
file.write('=======================================\n\n')
file.write('1. Front View (XY) \n' + oxy + '\n')
file.write('2. Top View (XZ) \n' + oxz + '\n')
file.write('3. Side View (YZ) \n' + oyz + '\n')
file.write('\n=======================================\n\n')
file.close()
