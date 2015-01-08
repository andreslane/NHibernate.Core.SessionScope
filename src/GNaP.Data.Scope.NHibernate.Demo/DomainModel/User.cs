namespace GNaP.Data.Scope.NHibernate.Demo.DomainModel
{
    using System;

    // Anemic model to keep this demo application simple.
	public  class User
	{
		public  virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Email { get; set; }
		public virtual int CreditScore { get; set; }
		public virtual bool WelcomeEmailSent { get; set; }
		public virtual DateTime CreatedOn { get; set; }

		public  override string ToString()
		{
			return String.Format("Id: {0} | Name: {1} | Email: {2} | CreditScore: {3} | WelcomeEmailSent: {4} | CreatedOn (UTC): {5}", Id, Name, Email, CreditScore, WelcomeEmailSent, CreatedOn.ToString("dd MMM yyyy - HH:mm:ss"));
		}
	}
}
