using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

public class OrganizationInvitation
{
    public int OrganizationInvitationId { get; set; }

    // Organisasjonen den ansatte inviteres til.
    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    // E-postadressen invitasjonen er ment for.
    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    // Kun hash av invitasjonskoden lagres.
    [Required]
    [MaxLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    // Brukeren som opprettet invitasjonen.
    [Required]
    public string InvitedByUserId { get; set; } = string.Empty;

    public ApplicationUser InvitedByUser { get; set; } = null!;

    // Tidspunkter lagres i UTC.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    // Null betyr at invitasjonen ikke er brukt.
    public DateTime? AcceptedAt { get; set; }

    // Gir mulighet til å trekke tilbake en invitasjon.
    public DateTime? RevokedAt { get; set; }
}