echo " Welcome to docker build"
echo ""
echo ""

$ABP_HOST="dannychen98/tfs_api:v1.0.0"
$ABP_HOST_DOCKERFILE_PATH="src/SND.SMP.Web.Host/Dockerfile"
$SMP_DISPATCH_DOCKERFILE_PATH="src/SND.SMP.DispatchConsole/Dockerfile"
$ABP_NG="dannychen98/tfs_ms:v1.0.0"
$SMP_DISPATCH="dannychen98/tfs_dispatch_console:v1.0.0"

cd ..
echo " Building docker image $SMP_DISPATCH..."
docker build -t $SMP_DISPATCH -f $SMP_DISPATCH_DOCKERFILE_PATH . 
echo " Done. -- Building docker image $SMP_DISPATCH..."
echo ""
echo ""

# echo " Pushing docker image $SMP_DISPATCH..."
# docker push $SMP_DISPATCH
# echo " Done. -- Pushing docker image $SMP_DISPATCH..."
# echo ""
# echo ""

# cd ..
# echo " Building docker image $ABP_HOST..."
# docker build -t $ABP_HOST -f $ABP_HOST_DOCKERFILE_PATH . 
# echo " Done. -- Building docker image $ABP_HOST..."
# echo ""
# echo ""

# echo " Pushing docker image $ABP_HOST..."
# docker push $ABP_HOST
# echo " Done. -- Pushing docker image $ABP_HOST..."
# echo ""
# echo ""

# cd ..
# cd ..
# cd angular/
# echo " Building docker image $ABP_NG..."
# docker build -t $ABP_NG -f Dockerfile .
# echo " Done. -- Building docker image $ABP_NG..."
# echo ""
# echo ""

# echo " Pushing docker image $ABP_NG..."
# docker push $ABP_NG
# echo " Done. -- Pushing docker image $ABP_NG..."
# echo ""
# echo ""


