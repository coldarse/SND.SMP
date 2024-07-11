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
  countries: string[];
}

export interface Zip {
  blob: Blob;
  filename: string;
}

export interface Stage4Update {
  dispatchNo: string;
  countryWithAirports: CountryWithAirport[];
}

export interface CountryWithAirport {
  country: string;
  airport: string;
  date: string;
}
