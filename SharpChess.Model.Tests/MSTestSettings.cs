//#if DEBUG
//[assembly: DoNotParallelize]
//#else
//[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
//#endif

// Some tests do fail when run in parallel,
// so => disable parallelization for now.
[assembly: DoNotParallelize]
