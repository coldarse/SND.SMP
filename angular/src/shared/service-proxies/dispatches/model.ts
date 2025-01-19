import type { PagedAndSortedResultRequestDto, EntityDto } from "@abp/ng.core";
import { BagDto } from "../bags/model";

export interface DispatchDto extends EntityDto<number> {
  customerCode: string;
  pOBox: string;
  pPI: string;
  postalCode: string;
  serviceCode: string;
  productCode: string;
  dispatchDate: string;
  dispatchNo: string;
  eTAtoHKG: string;
  flightTrucking: string;
  batchId: string;
  isPayment: boolean;
  noofBag: number;
  itemCount: number;
  totalWeight: number;
  totalPrice: number;
  status: number;
  isActive: boolean;
  cN38: string;
  transactionDateTime: string;
  aTA: string;
  postCheckTotalBags: number;
  postCheckTotalWeight: number;
  airportHandling: number;
  remark: string;
  weightGap: number;
  weightAveraged: number;
  dateSOAProcessCompleted: string;
  sOAProcessCompletedByID: number;
  totalWeightSOA: number;
  totalAmountSOA: number;
  performanceDaysDiff: number;
  datePerformanceDaysDiff: string;
  airlineCode: string;
  flightNo: string;
  portDeparture: string;
  extDispatchNo: string;
  dateFlight: string;
  airportTranshipment: string;
  officeDestination: string;
  officeOrigin: string;
  stage1StatusDesc: string;
  stage2StatusDesc: string;
  stage3StatusDesc: string;
  stage4StatusDesc: string;
  stage5StatusDesc: string;
  stage6StatusDesc: string;
  stage7StatusDesc: string;
  stage8StatusDesc: string;
  stage9StatusDesc: string;
  dateStartedAPI: string;
  dateEndedAPI: string;
  statusAPI: string;
  countryOfLoading: string;
  dateFlightArrival: string;
  postManifestSuccess: boolean;
  postManifestMsg: string;
  postManifestDate: string;
  postDeclarationSuccess: boolean;
  postDeclarationMsg: string;
  postDeclarationDate: string;
  airwayBLNo: string;
  airwayBLDate: string;
  dateLocalDelivery: string;
  dateCLStage1Submitted: string;
  dateCLStage2Submitted: string;
  dateCLStage3Submitted: string;
  dateCLStage4Submitted: string;
  dateCLStage5Submitted: string;
  dateCLStage6Submitted: string;
  bRCN38RequestId: string;
  dateArrival: string;
  dateAcceptanceScanning: string;
  seqNo: number;
  cORateOptionId: string;
  paymentMode: string;
  currencyId: string;
  importProgress: number;
}

export interface PagedDispatchResultRequestDto
  extends PagedAndSortedResultRequestDto {
  keyword?: string;
  customerCode?: string;
  pOBox?: string;
  pPI?: string;
  postalCode?: string;
  serviceCode?: string;
  productCode?: string;
  dispatchNo?: string;
  flightTrucking?: string;
  batchId?: string;
  cN38?: string;
  transactionDateTime?: string;
  aTA?: string;
  remark?: string;
  dateSOAProcessCompleted?: string;
  datePerformanceDaysDiff?: string;
  airlineCode?: string;
  flightNo?: string;
  portDeparture?: string;
  extDispatchNo?: string;
  dateFlight?: string;
  airportTranshipment?: string;
  officeDestination?: string;
  officeOrigin?: string;
  stage1StatusDesc?: string;
  stage2StatusDesc?: string;
  stage3StatusDesc?: string;
  stage4StatusDesc?: string;
  stage5StatusDesc?: string;
  stage6StatusDesc?: string;
  stage7StatusDesc?: string;
  stage8StatusDesc?: string;
  stage9StatusDesc?: string;
  dateStartedAPI?: string;
  dateEndedAPI?: string;
  statusAPI?: string;
  countryOfLoading?: string;
  dateFlightArrival?: string;
  postManifestMsg?: string;
  postManifestDate?: string;
  postDeclarationMsg?: string;
  postDeclarationDate?: string;
  airwayBLNo?: string;
  airwayBLDate?: string;
  dateLocalDelivery?: string;
  dateCLStage1Submitted?: string;
  dateCLStage2Submitted?: string;
  dateCLStage3Submitted?: string;
  dateCLStage4Submitted?: string;
  dateCLStage5Submitted?: string;
  dateCLStage6Submitted?: string;
  bRCN38RequestId?: string;
  dateArrival?: string;
  dateAcceptanceScanning?: string;
  cORateOptionId?: string;
  paymentMode?: string;
  currencyId?: string;
  isAdmin?: boolean;
}

export interface GetPostCheck {
  companyName: string;
  dispatchNo: string;
  flightTrucking: string;
  eta: Date;
  ata: Date;
  companyCode: string;
  preCheckNoOfBag: number;
  postCheckNoOfBag: number;
  preCheckWeight: number;
  postCheckWeight: number;
  serviceCode: string;
  bags: BagDto[];
}

export interface DispatchInfoDto {
  customerName: string;
  customerCode: string;
  postalCode: string;
  postalDesc: string;
  dispatchDate: string;
  dispatchNo: string;
  serviceCode: string;
  serviceDesc: string;
  productCode: string;
  productDesc: string;
  totalBags: number;
  totalWeight: number;
  totalCountry: number;
  status: string;
  path: string;
  importProgress: number;
  remark: string;
}

export interface Zip {
  blob: Blob;
  filename: string;
}

export interface DispatchBag {
  bagId: number;
  bagNo: string;
  itemCount: number;
  select: boolean;
  custom: boolean;
  stages: Stage;
}

export interface DispatchCountry {
  bagCount: number;
  countryCode: string;
  dispatchBags: DispatchBag[];
  open: boolean;
  select: boolean;
  stages: Stage;
}

export interface DispatchInfo {
  dispatch: string;
  dispatchId: number;
  dispatchDate: string;
  postalCode: string;
  status: number;
  customer: string;
  open: boolean;
  dispatchCountries: DispatchCountry[];
}

export interface DispatchTracking {
  dispatches: DispatchInfo[];
  countries: string[]
}

export interface Stage {
  stage1Desc: string;
  stage2Desc: string;
  stage3Desc: string;
  stage4Desc: string;
  stage5Desc: string;
  stage6Desc: string;
  stage7Desc: string;
  stage1DateTime: string;
  stage2DateTime: string;
  stage3DateTime: string;
  stage4DateTime: string;
  stage5DateTime: string;
  stage6DateTime: string;
  stage7DateTime: string;
  airport: string;
  airportDateTime: string;
  bagNo: string;
  dispatchNo: string;
  countryCode: string;
}

export interface DispatchDetails {
  date: string; 
  name: string;
  weight: number;
  credit: number;
  debit: number;
  selected: boolean;
  itemCount: number;
}

export interface SimplifiedItem {
  dispatchNo: string;
  weight: number;
  country: string;
  identifier: string;
  rate: number;
  quantity: number;
  unitPrice: number;
  amount: number;
  productCode: string;
  currency: string;
  initial: boolean;
}

export interface CustomerDispatchDetails {
  details: DispatchDetails[];
  address: string;
}

export interface ItemWrapper {
  dispatchItems: SimplifiedItem[];
  surchargeItems: SimplifiedItem[];
  totalAmount: number;
  totalAmountWithSurcharge: string;
}


