using AutoMapper;
using Datahub.TrainingPortal.Application.DTOs.Assignments;
using Datahub.TrainingPortal.Application.DTOs.Departments;
using Datahub.TrainingPortal.Application.DTOs.Enrollments;
using Datahub.TrainingPortal.Application.DTOs.Evaluations;
using Datahub.TrainingPortal.Application.DTOs.Materials;
using Datahub.TrainingPortal.Application.DTOs.Trainings;
using Datahub.TrainingPortal.Application.DTOs.Users;
using Datahub.TrainingPortal.Domain.Entities;

namespace Datahub.TrainingPortal.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department != null ? s.Department.Name : null));

        CreateMap<Department, DepartmentDto>()
            .ForMember(d => d.UserCount, o => o.MapFrom(s => s.Users.Count))
            .ForMember(d => d.TrainingCount, o => o.MapFrom(s => s.Trainings.Count));

        CreateMap<Training, TrainingDto>()
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department != null ? s.Department.Name : null))
            .ForMember(d => d.TrainerName, o => o.MapFrom(s => s.Trainer != null ? s.Trainer.FullName : null))
            .ForMember(d => d.MaterialCount, o => o.MapFrom(s => s.Materials.Count))
            .ForMember(d => d.EnrollmentCount, o => o.MapFrom(s => s.Enrollments.Count));

        CreateMap<MaterialFileItem, MaterialFileItemDto>();
        CreateMap<MaterialLinkItem, MaterialLinkItemDto>();

        CreateMap<TrainingMaterial, TrainingMaterialDto>()
            .ForMember(d => d.UploadedByUserName, o => o.MapFrom(s => s.UploadedByUser != null ? s.UploadedByUser.FullName : null));

        CreateMap<AssignmentAttachment, AssignmentAttachmentDto>();
        CreateMap<AssignmentSubmissionAttachment, AssignmentAttachmentDto>();

        CreateMap<Assignment, AssignmentDto>()
            .ForMember(d => d.CreatedByUserName, o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.FullName : null))
            .ForMember(d => d.Status, o => o.Ignore());

        CreateMap<AssignmentSubmission, AssignmentSubmissionDto>();

        CreateMap<EvaluationCategoryScore, EvaluationCategoryScoreDto>();

        CreateMap<Evaluation, EvaluationDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.FullName : null))
            .ForMember(d => d.EvaluatedByUserName, o => o.MapFrom(s => s.EvaluatedByUser != null ? s.EvaluatedByUser.FullName : null))
            .ForMember(d => d.TrainingId, o => o.MapFrom(s => s.Enrollment != null ? s.Enrollment.TrainingId : (Guid?)null))
            .ForMember(d => d.TrainingTitle, o => o.MapFrom(s => s.Enrollment != null && s.Enrollment.Training != null ? s.Enrollment.Training.Title : null))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s =>
                s.Enrollment != null && s.Enrollment.Training != null && s.Enrollment.Training.Department != null
                    ? s.Enrollment.Training.Department.Name
                    : s.User != null && s.User.Department != null ? s.User.Department.Name : null))
            .ForMember(d => d.Total, o => o.MapFrom(s => s.Categories.Sum(c => c.Score)));

        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(d => d.TrainingTitle, o => o.MapFrom(s => s.Training != null ? s.Training.Title : null))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.FullName : null))
            .ForMember(d => d.AssignedByUserName, o => o.MapFrom(s => s.AssignedByUser != null ? s.AssignedByUser.FullName : null));
    }
}
