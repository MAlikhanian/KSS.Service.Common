using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class AddressLabelTranslationController : BaseController<AddressLabelTranslation, AddressLabelTranslationDto, AddressLabelTranslationDto, AddressLabelTranslationDto>
    {
        public AddressLabelTranslationController(IAddressLabelTranslationService service) : base(service) { }
    }
}
