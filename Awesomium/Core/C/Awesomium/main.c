#include <Awesomium/awesomium_capi.h>

int main(int argc, char** argv)
{

#ifdef _WIN32
	if(awe_is_child_process(GetModuleHandle(0)))
		return awe_child_process_main(GetModuleHandle(0));
#else
	if(awe_is_child_process(argc, argv)
		return awe_child_process_main(argc, argv);
#endif

	return 0;
}