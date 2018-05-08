#import <thread>
#include <time.h>
#include <math.h>
#include "FalconWallTest.h"
#include"../falcon_mouse/falcon_mouse.h"
#include"../falcon_mouse/falcon_mouse_osx.cpp" // hardcoded for macOS right now

#include "falcon/core/FalconDevice.h"
#include "falcon/grip/FalconGripFourButton.h"
#include "falcon/kinematic/FalconKinematicStamper.h"

time_t start_t, end_t;// = clock();
float force_decay = 1;
bool threadStarted = false, inputForcePresent = false, inputDeltaPresent = false;
std::array<double, 3> inputForce = {0,0,0};
std::array<double, 3> addedForce = {0,0,0};
std::array<double, 3> inputDelta = {0,0,0};
std::array<double, 3> addedDelta = {0,0,0};
void threadFunc()
{
	std::cout << "Starting input listener thread...\n";
    while (true) {
		std::string in;
		std::getline(std::cin, in);
		if (in.length() <= 2)
			continue;
		int ind1 = in.find_first_of(",");
		int ind2 = in.find_last_of(",");
		if (ind1 != -1 && ind2 != -1) {
			try {
				if (in[0] == 'f') {
					inputForce[0] = stof(in.substr(2,ind1));
					inputForce[1] = stof(in.substr(ind1+1,ind2-ind1));
					inputForce[2] = stof(in.substr(ind2+1,in.length()-ind2));
					//std::cout << "Received force: (" << inputForce[0] << ", " << inputForce[1] << ", " << inputForce[2] << ")\n";
					inputForcePresent = true;
				} else if (in[0] == 'd') {
					inputDelta[0] = stof(in.substr(2,ind1));
					inputDelta[1] = stof(in.substr(ind1+1,ind2-ind1));
					inputDelta[2] = stof(in.substr(ind2+1,in.length()-ind2));
					//std::cout << "Received delta: (" << inputDelta[0] << ", " << inputDelta[1] << ", " << inputDelta[2] << ")\n";
					inputDeltaPresent = true;
				} else
					std::cout << "Invalid force: " << in << "\n";
			}
			catch(std::invalid_argument& e) {
		  		// no conversion could be performed
				std::cout << "Invalid force: " << in << "\n";
			}
		} else {
			std::cout << "Invalid force: " << in << "\n";
		}
	}
	//threadStarted = false;
}

FalconWallTest::FalconWallTest(std::shared_ptr<libnifalcon::FalconDevice> d, unsigned int axis) :
	FalconTestBase(d),
	m_axisBounds(0, 0, .130),
	m_stiffness(1000),
	m_isInitializing(true),
	m_hasPrintedInitMsg(false),
	m_axis(axis),
	m_positiveForce(true),
	m_runClickCount(0),
	m_buttonDown(false)
{
	m_falconDevice->setFalconKinematic<libnifalcon::FalconKinematicStamper>();
	m_falconDevice->setFalconGrip<libnifalcon::FalconGripFourButton>();

}

void FalconWallTest::runMouseLoop()
{
	m_falconDevice->setFalconKinematic<libnifalcon::FalconKinematicStamper>();
	//m_falconDevice.startThread();
	std::array<double, 3> d;
	//int i = 0, j = 0;
	int old_x = 0, old_y = 0, x = 0, y = 0;
	int width = getDisplayWidth();
	int height = getDisplayHeight();
	while(1)//!stop)
	{
		//This math doesn't map the workspace to the window properly yet.
		if(!m_falconDevice->runIOLoop())
			continue;
		d = m_falconDevice->getPosition();
		old_x = x;
		old_y = y;
		x = ((d[0] + .06) / .12) * width;
		y = (((.12 - (d[1] + .06)) / .12) * height);

		if(old_x != x || old_y != y)
			setMousePosition(x, y);
	}
	//m_falconDevice.stopThread();
}

int old_x = 0, old_y = 0, x = 0, y = 0;
int width = getDisplayWidth();
int height = getDisplayHeight();

void FalconWallTest::runFunction()
{
	if (!threadStarted) {
		std::thread thread_obj(threadFunc);
		thread_obj.detach();
		//thread_obj.join();
		threadStarted = true;
		start_t = clock();
	}

	if(!m_falconDevice->runIOLoop())
		return;

	std::array<double, 3> pos = m_falconDevice->getPosition();

	old_x = x;
	old_y = y;
	x = ((pos[0] + .06) / .12) * width;
	y = (((.12 - (pos[1] + .06)) / .12) * height);

	if(old_x != x || old_y != y)
		setMousePosition(width-x, height-y);

	if(m_isInitializing)
	{
		if(!m_hasPrintedInitMsg)
		{
			std::cout << "Move the end effector out of the wall area" << std::endl;
			m_hasPrintedInitMsg = true;
		}
		if(((pos[m_axis] > m_axisBounds[m_axis]) && m_positiveForce) || ((pos[m_axis] < m_axisBounds[m_axis]) && !m_positiveForce))
		{
			std::cout << "Starting wall simulation... Press center button (circle button) to change direction of force..." << std::endl;
			m_isInitializing = false;
			tstart();
		}
		return;
	}

	//Cheap debounce
	if(m_falconDevice->getFalconGrip()->getDigitalInputs() & libnifalcon::FalconGripFourButton::BUTTON_3)
	{
		m_buttonDown = true;
	}
	else if(m_buttonDown)
	{
		m_buttonDown = false;
		std::cout << "Flipping force vector..." << std::endl;
		m_runClickCount = m_lastLoopCount;
		m_positiveForce = !m_positiveForce;
		m_hasPrintedInitMsg = false;
		m_isInitializing = true;
		return;
	}

	std::array<double, 3> force = {0,0,0};

	double dist = 10000;
	int closest = -1, outside=3;

	force[0] = -m_stiffness*pos[0]/50.0;
	force[1] = -m_stiffness*pos[1]/50.0;

	// For each axis, check if the end effector is inside
	// the cube.  Record the distance to the closest wall.

	force[m_axis] = 0;
	if(((pos[m_axis] < m_axisBounds[m_axis]) && m_positiveForce) || ((pos[m_axis] > m_axisBounds[m_axis]) && !m_positiveForce))
	{
		double dA = pos[m_axis]-m_axisBounds[m_axis];
		dist = dA;
		closest = m_axis;
	}

	// If so, add a proportional force to kick it back
	// outside from the nearest wall.

	if (closest > -1)
		force[closest] = -m_stiffness*dist;

	// check for force data from Unity
	if (inputForcePresent) {
		std::cout << "Applying force: (" << inputForce[0] << ", " << inputForce[1] << ", " << inputForce[2] << ")\n";
		inputForcePresent = false;
		start_t = clock();
		double range = 6.0;
		addedForce[0] = gmtl::Math::clamp(inputForce[0],-range, range);
		addedForce[1] = gmtl::Math::clamp(inputForce[1],-range, range);
		addedForce[2] = gmtl::Math::clamp(inputForce[2],-range, range);
	}
	if (inputDeltaPresent) {
		std::cout << "Applying delta: (" << inputDelta[0] << ", " << inputDelta[1] << ", " << inputDelta[2] << ")\n";
		inputDeltaPresent = false;
		double range = 3.0;
		double prescale = .5;
		addedDelta[0] = gmtl::Math::clamp(inputDelta[0]*prescale,-range, range);
		addedDelta[1] = gmtl::Math::clamp(inputDelta[1]*prescale,-range, range);
		addedDelta[2] = gmtl::Math::clamp(inputDelta[2]*prescale,-range, range);
	}

	// update force timings
	end_t = clock();
   	double delta_t = (double)(end_t - start_t) / CLOCKS_PER_SEC;

	double maxTimeSecs = .02;
   	double cycle = gmtl::Math::clamp(delta_t,0.0,maxTimeSecs);
   	cycle *= (1/maxTimeSecs) *3.1415; // normalize to range (0,PI)
	cycle += 3.1415/2; // offset by PI/2
	double sinVal = (sin(cycle)+1.0)/2.0; // force value is a sine decay curve from 1 to 0 over the time maxTimeSecs
	force[0] += addedForce[0] * sinVal;
	force[1] += addedForce[1] * sinVal;
	force[2] += addedForce[2] * sinVal;

	force[0] += addedDelta[0];
	force[1] += addedDelta[1];
	force[2] += addedDelta[2];

	m_falconDevice->setForce(force);
}
