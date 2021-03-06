using System;

namespace Light.Common
{
	public enum Outcomes : byte
	{
		Unknown = 0, // (Default) For ex. a network timeout may suggest the operation has or has not succeeded.

		NotAvailable = 1, // Resource or item was not available at the requested location.

		Success = 2, // !! The only success outcome is this one.

		Failure = 3,

		NotSupported = 4, // Operation not supported.

		Aborted = 5, // Process got aborted.

		TimedOut = 6, // Process timed out.
	}

	public interface IResponse
	{
		Outcomes Outcome { get; set; }

		/// <summary>
		/// Make int?
		/// </summary>
		int Code { get; set; }

		string Message { get; set; }

		bool IsSuccess { get; }

		bool IsTotalSuccess { get; }

		bool IsNotSuccess { get; }

		bool IsNotTotalSuccess { get; }

		bool IsExecuted { get; }
	}

	public struct Response : IResponse
	{
		/// <summary>
		/// Rename to Status
		/// </summary>
		public Outcomes Outcome { get; set; }

		/// <summary>
		/// TODO: make int?
		/// </summary>
		public int Code { get; set; }

		/// <summary>
		/// Detail info on how the operation went.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Success or Partial success.
		/// </summary>
		public bool IsSuccess
		{
			get { return Outcome == Outcomes.Success; }
		}

		/// <summary>
		/// Generic success result.
		/// </summary>
		public static Response Success
		{
			get { return new Response(Outcomes.Success); }
		}

		/// <summary>
		/// Generic failure result.
		/// </summary>
		public static Response Failure
		{
			get { return new Response(Outcomes.Success); }
		}

		/// <summary>
		///
		/// </summary>
		public bool IsTotalSuccess
		{
			get { return Outcome == Outcomes.Success; }
		}

		/// <summary>
		/// This is not Equal to Failure; can also be unknown.
		/// </summary>
		public bool IsNotSuccess
		{
			get { return Outcome != Outcomes.Success; }
		}

		/// <summary>
		/// This is not Equal to Failure; can also be unknown.
		/// </summary>
		public bool IsNotTotalSuccess
		{
			get { return Outcome != Outcomes.Success; }
		}

		public bool IsExecuted
		{
			get { return Outcome != Outcomes.Unknown; }
		}

		public Response(Outcomes value)
			: this()
		{
			Outcome = value;
		}

		public Response(Outcomes value, string message)
			: this()
		{
			Outcome = value;
			Message = message;
		}

		public Response(IResponse source)
			: this()
		{
			Outcome = source.Outcome;
			Message = source.Message;
			Code = source.Code;
		}

		public static Response Fail(string message)
		{
			return new Response(Outcomes.Failure) { Message = message };
		}

		public static Response Succeed(string message = null)
		{
			return new Response(Outcomes.Success) { Message = message };
		}

		public static Response Fail(Exception ex)
		{
			return new Response(Outcomes.Failure) { Message = ex.Message };
		}

		public override string ToString()
		{
			return string.Format("{0}, Code {1}, Msg {2}", Outcome, Code, Message);
		}
	}

	public struct Response<T> : IResponse
	{
		public Outcomes Outcome { get; set; }

		public int Code { get; set; }

		public string Message { get; set; }

		public T Result { get; set; }

		/// <summary>
		/// Success or Partial success.
		/// </summary>
		[Newtonsoft.Json.JsonIgnore]
		public bool IsSuccess
		{
			get { return Outcome == Outcomes.Success; }
		}

		/// <summary>
		///
		/// </summary>
		[Newtonsoft.Json.JsonIgnore]
		public bool IsTotalSuccess
		{
			get { return Outcome == Outcomes.Success; }
		}

		/// <summary>
		/// This is not Equal to Failure; can also be unknown.
		/// </summary>
		[Newtonsoft.Json.JsonIgnore]
		public bool IsNotSuccess
		{
			get { return Outcome != Outcomes.Success; }
		}

		/// <summary>
		/// This is not Equal to Failure; can also be unknown.
		/// </summary>
		[Newtonsoft.Json.JsonIgnore]
		public bool IsNotTotalSuccess
		{
			get { return Outcome != Outcomes.Success; }
		}

		//public bool IsFailureOrUnknown
		//{
		//	get { return Outcome != Outcomes.Success && Outcome != Outcomes.PartialSuccess /*&& Outcome != Outcomes.Unknown*/; }
		//}

		[Newtonsoft.Json.JsonIgnore]
		public bool IsExecuted
		{
			get { return Outcome != Outcomes.Unknown; }
		}

		public Response(Outcomes value)
			: this()
		{
			Outcome = value;
		}

		public Response(Outcomes value, string message)
			: this()
		{
			Outcome = value;
			Message = message;
		}

		public Response(T result)
			: this()
		{
			Outcome = Outcomes.Success;
			Result = result;
		}

		public static Response<T> Succeed(T value)
		{
			return new Response<T>(value);
		}

		public static Response<T> Fail(string message)
		{
			return new Response<T>(Outcomes.Failure, message);
		}

		public override string ToString()
		{
			return string.Format("{0}, Code {1}, Msg {2}, Result {3}", Outcome, Code, Message, Result);
		}
	}
}