echo " Welcome to docker build"
echo ""
echo ""

$ABP_HOST="dannychen98/smp_api:v1.2.38"
$ABP_NG="dannychen98/smp_ms:v1.2.26"
$ABP_HOST_DOCKERFILE_PATH="src/SND.SMP.Web.Host/Dockerfile"
$SMP_DISPATCH_1_DOCKERFILE_PATH="src/SND.SMP.DispatchImporter/Dockerfile"
$SMP_DISPATCH_2_DOCKERFILE_PATH="src/SND.SMP.DispatchValidator/Dockerfile"
$SMP_DISPATCH_3_DOCKERFILE_PATH="src/SND.SMP.DispatchTrackingUpdater/Dockerfile"
$SMP_DISPATCH_4_DOCKERFILE_PATH="src/SND.SMP.ItemTrackingGenerator/Dockerfile"
$SMP_DISPATCH_5_DOCKERFILE_PATH="src/SND.SMP.ItemTrackingUpdater/Dockerfile"
$SMP_DISPATCH_1="dannychen98/dispatch_importer:v1.0.1"
$SMP_DISPATCH_2="dannychen98/dispatch_validator:v1.0.4"
$SMP_DISPATCH_3="dannychen98/dispatch_tracking_updater:v1.0.1"
$SMP_DISPATCH_4="dannychen98/item_tracking_generator:v1.0.1"
$SMP_DISPATCH_5="dannychen98/item_tracking_updater:v1.0.1"

# cd ..
# echo " Building docker image $SMP_DISPATCH_1..."
# docker build -t $SMP_DISPATCH_1 -f $SMP_DISPATCH_1_DOCKERFILE_PATH . 
# echo " Done. -- Building docker image $SMP_DISPATCH_1..."
# echo ""
# echo ""

cd ..
echo " Building docker image $SMP_DISPATCH_2..."
docker build -t $SMP_DISPATCH_2 -f $SMP_DISPATCH_2_DOCKERFILE_PATH . 
echo " Done. -- Building docker image $SMP_DISPATCH_2..."
echo ""
echo ""

# cd ..
# echo " Building docker image $SMP_DISPATCH_3..."
# docker build -t $SMP_DISPATCH_3 -f $SMP_DISPATCH_3_DOCKERFILE_PATH . 
# echo " Done. -- Building docker image $SMP_DISPATCH_3..."
# echo ""
# echo ""

# cd ..
# echo " Building docker image $SMP_DISPATCH_4..."
# docker build -t $SMP_DISPATCH_4 -f $SMP_DISPATCH_4_DOCKERFILE_PATH . 
# echo " Done. -- Building docker image $SMP_DISPATCH_4..."
# echo ""
# echo ""

# cd ..
# echo " Building docker image $SMP_DISPATCH_5..."
# docker build -t $SMP_DISPATCH_5 -f $SMP_DISPATCH_5_DOCKERFILE_PATH . 
# echo " Done. -- Building docker image $SMP_DISPATCH_5..."
# echo ""
# echo ""

# cd ..
# echo " Building docker image $ABP_HOST..."
# docker build -t $ABP_HOST -f $ABP_HOST_DOCKERFILE_PATH . 
# echo " Done. -- Building docker image $ABP_HOST..."
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


