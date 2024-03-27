using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bb.Models
{

    public class ContractModel
    {

        public ContractModel()
        {
            Diagnostic = new List<Diagnostic>();
            Templates = new List<string>();
        }

        public List<Diagnostic> Diagnostic { get; set; }

        public List<string> Templates { get; set; }

    }

    public class  Diagnostic
    {

        public Diagnostic(string Severity, string message)
        {
            this.Severity = Severity;
            this.Message = message;
        }

        public Diagnostic()
        {
                
        }

        public string? Location { get; internal set; }
        
        public string Severity { get; }
        
        public string Message { get; }

    }


}
