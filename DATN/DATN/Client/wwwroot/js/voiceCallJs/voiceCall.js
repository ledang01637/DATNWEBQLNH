var token = 'eyJjdHkiOiJzdHJpbmdlZS1hcGk7dj0xIiwidHlwIjoiSldUIiwiYWxnIjoiSFMyNTYifQ.eyJqdGkiOiJTSy4wLjZoSHpGNDlFTHlQMDlUc0Q4VTAwbENERlhEMTdQeS0xNzI1MjQ3NTExIiwiaXNzIjoiU0suMC42aEh6RjQ5RUx5UDA5VHNEOFUwMGxDREZYRDE3UHkiLCJleHAiOjE3MjUzMzM5MTEsInVzZXJJZCI6InJlZ3VsYXJfY2xpZW50In0.FL0rDdDNBMb3cXBEDoWucpfd7q0eHZPUqFKCfI1xoxk';
var callerId = 'regular_client';
var calleeId = 'vip_agent';

$("#callButton").click(function () {
    localVideo.muted = false;
    remoteVideo.muted = false;
    localVideo.play();
});