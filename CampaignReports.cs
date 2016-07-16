using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace reports_tcado
{
    public class CampaignSummary
    {
        public DateTime LeadDate { get; set; }
        public string CompanyName { get; set; }
        public int LeadCount { get; set; }

        public static IEnumerable<CampaignSummary> GetSummary(DateTime startdate, DateTime enddate, int category)
        {
            using (var db = new PetaPoco.Database("LOXPressDatabase"))
            {
                var sql = @"
select CAST(f.DateCreated as Date) as LeadDate, CompanyName as LeadSource, Count(*) as LeadCount
from Borrower b
inner join [File] f on f.BorrowerID=b.ID and f.DateCreated > @0 and f.DateCreated < @1
inner join [Status] st on st.ID=f.StatusID
inner join Campaign c on c.ID=b.CampaignID
inner join CampaignSource s on s.ID=c.SourceID
inner join [Group] g on g.ID=c.OwningGroupID and (g.ID IN (1,21,22) or g.ParentGroupID IN (1, 21, 22))
where s.CategoryID=@2
group by CAST(f.DateCreated as Date), CompanyName
order by 1, 2";
                return db.Fetch<CampaignSummary>(sql, startdate, enddate, category);
            }
        }
    }

    public class CampaignDetails
    {
        public DateTime LeadDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string LeadSource { get; set; }
        public string Status { get; set; }

        public static IEnumerable<CampaignDetails> GetDetails(DateTime startdate, DateTime enddate, int category)
        {
            using (var db = new PetaPoco.Database("LOXPressDatabase"))
            {
                var sql = @"
select CAST(f.DateCreated as Date) as LeadDate, b.Firstname, b.LastName, b.Phone, OfferCode as LeadSource, st.Name as [Status]
from Borrower b
inner join [File] f on f.BorrowerID=b.ID and f.DateCreated > @0 and f.DateCreated < @1
inner join [Status] st on st.ID=f.StatusID
inner join Campaign c on c.ID=b.CampaignID
inner join CampaignSource s on s.ID=c.SourceID
inner join [Group] g on g.ID=c.OwningGroupID and (g.ID IN (1,21,22) or g.ParentGroupID IN (1, 21, 22))
where s.CategoryID=@2
order by 1
";
                return db.Fetch<CampaignDetails>(sql, startdate, enddate, category);
            }
        }
    }
}
