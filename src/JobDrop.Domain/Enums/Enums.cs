namespace JobDrop.Domain.Enums;

public enum DropBucket { Daily = 1, Bonus = 2, Special = 3 }
public enum TravelMode { ScheduledTransit = 1, Car = 2, Walking = 3, Micromobility = 4 }
public enum WorkMode { OnSite = 1, Hybrid = 2, Remote = 3 }
public enum VerificationLevel { Unverified = 0, TradeLicense = 1, PhysicalNode = 2 }
public enum ApplicationStatus { Queued = 0, Delivered = 1, Opened = 2, Reviewed = 3, Shortlisted = 4, InterviewScheduled = 5, OfferExtended = 6, Rejected = 7, ExpiredRefunded = 8, Withdrawn = 9, Cancelled = 10 }
public enum SlaStatus { Pending = 0, Responded = 1, ExpiredRefunded = 2, Cancelled = 3 }
public enum ReverseDropStatus { Sent = 0, Accepted = 1, Declined = 2, ExpiredRefunded = 3 }
public enum CompanyMemberRole { Owner = 0, Admin = 1, Recruiter = 2, HiringManager = 3, Viewer = 4 }
public enum DocumentScanStatus { PendingScan = 0, Clean = 1, Quarantined = 2 }
public enum OfficeType { HQ = 1, SatelliteHub = 2, RecruitingBooth = 3 }
public enum ActorType { User = 0, System = 1, Admin = 2 }
public enum PaymentStatus { Created = 0, Pending = 1, Paid = 2, Failed = 3, Refunded = 4, Voided = 5 }
public enum PortalMode { JobSeeker = 1, Recruiter = 2 }
public enum JobStatus { Draft = 0, Published = 1, Paused = 2, Closed = 3 }